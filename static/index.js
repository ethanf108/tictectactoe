const drawElement = document.getElementById("draw");

const width = drawElement.width;
const height = drawElement.height;

const draw = drawElement.getContext("2d");

let mouseX = 0, mouseY = 0;

let hoverPiece = null;
let inBox = false;

let board = null;

let pieceToPlay = null;

let gameID = null;
let playerID = null;
let status = null;

const drawBG = () => {
    draw.fillStyle = "gray";
    draw.fillRect(0, 0, width, height);
    
    for(let x = 0; x < 4; x++){
	for(let y = 0; y < 4; y++){

	    const isMouseWithinBounds =
		  Math.pow((x + 1) * width / 5 - mouseX, 2)
		  + Math.pow((y + 1) * height / 5 - mouseY, 2)
		  <= Math.pow(width / 12, 2);

	    if (isMouseWithinBounds) {
		hoverPiece = {
		    x: x,
		    y: y,
		};
	    }
	    
	    draw.strokeStyle = isMouseWithinBounds ? "green" : "yellow";
	    draw.lineWidth = 1;
	    draw.beginPath();
	    draw.arc((x + 1) * width / 5, (y + 1) * height / 5, width / 12, 0, 2 * Math.PI);
	    draw.stroke();
	}
    }
}

const drawBoard = () => {
    drawBG();

    for(let x = 0; x < 4; x++){
	for(let y = 0; y < 4; y++){    
	    drawPiece(x, y, board[x + y * 4]);
	}
    }
    
    draw.fillStyle = "yellow";
    draw.font = "24pt sans-serif";
    if (!status.endsWith(playerID) && !status.startsWith("WON")) {
	draw.fillText("Waiting for move", width/6, 50);
    } else if (!status.startsWith("WON")) {
	if (pieceToPlay === null) {
	    draw.fillText("Your turn", width/6, 50);
	} else {
	    draw.fillText("Place piece", width/6, 50);
	}
	draw.fillStyle = draw.strokeStyle = "lightgreen";
	draw.fillText("Pick Piece >", width * 0.32, height * 0.96);
	if (inBox) {
	    draw.strokeStyle = "green";
	}
	draw.strokeRect(width * 0.3, height * 0.9, width * 0.4, height * 0.08);
    }
}

const iToPiece = i => i | ((i ^ 15) << 4);

const drawPickBoard = () => {
    drawBG();
    
    for(let x = 0; x < 4; x++){
	for(let y = 0; y < 4; y++){
	    const piece = iToPiece(x + 4 * y);
	    if (board.includes(piece)) {
		continue;
	    }
	    drawPiece(x, y, piece);
	}
    }
    
    draw.fillStyle = "yellow";
    draw.font = "24pt sans-serif";
    draw.fillText("Pick a piece", width/4.5, 50);
    
    draw.fillStyle = draw.strokeStyle = "lightgreen";
    draw.fillText("< View Board", width * 0.31, height * 0.96);
    if (inBox) {
	draw.strokeStyle = "green";
    }
    draw.strokeRect(width * 0.3, height * 0.9, width * 0.4, height * 0.08);
}

const drawPiece = (x, y, piece) => {
    if (piece === 0) {
	return;
    }
    
    const tall = piece & 1 > 0;
    const circle = piece >>> 1 & 1 > 0;
    const red = piece >>> 2 & 1 > 0;
    const filled = piece >>> 3 & 1 > 0;

    draw.fillStyle = draw.strokeStyle = red ? "red" : "blue";
    draw.lineWidth = 2;
    
    if (circle) {
	draw.beginPath();
	draw.arc((x + 1) * width / 5, (y + 1) * height / 5, width / (tall ? 18 : 30), 0, 2 * Math.PI);
	if (filled) {
	    draw.fill();
	} else {
	    draw.stroke();
	}
    } else {
	if (filled) {
	    draw.fillRect(
		(x + 1) * width / 5 - width / (tall ? 20 : 30),
		(y + 1) * height / 5 - height / (tall ? 20 : 30),
		width / (tall ? 10 : 15),
		height / (tall ? 10 : 15));
	} else {
	    draw.strokeRect(
		(x + 1) * width / 5 - width / (tall ? 20 : 30),
		(y + 1) * height / 5 - height / (tall ? 20 : 30),
		width / (tall ? 10 : 15),
		height / (tall ? 10 : 15));
	}
    }
}

const drawMain = () => {
    if (!gameID) {
	document.getElementById("new").hidden=false;
    } else {
	document.getElementById("new").hidden=true;
	document.getElementById("copy").hidden=true;
	if (status === "WAITING") {
	    document.getElementById("copy").hidden=false;
	    drawBG();
	    draw.fillStyle = "yellow";
	    draw.font = "24pt sans-serif";
	    draw.fillText("Waiting for player to join", width/10, 50);
	} else if (status.startsWith("WON")) {
	    drawBoard();
	    draw.fillStyle = "yellow";
	    draw.font = "36pt sans-serif";
	    draw.fillText(`You ${status.endsWith(playerID) ? "Won!" : "Lost"}`, width/3.5, 50);
	    document.getElementById("new").hidden=false;
	} else {
	    if (pieceToPlay === 0 && status.endsWith(playerID)) {
		drawPickBoard();
	    } else {
		drawBoard();
	    }
	}
    }
}

const mouseMove = evt => {
    const rect = drawElement.getBoundingClientRect();
    
    const x = evt.clientX - rect.left;
    const y = evt.clientY - rect.top;

    mouseX = x;
    mouseY = y;

    hoverPiece = null;

    inBox = x >= width * 0.3
	&& x <= width * 0.7
	&& y >= height * 0.9
	&& y <= height * 0.98;

    drawMain();
}

drawElement.addEventListener("mousemove", mouseMove, false);

drawElement.addEventListener("click", evt => {
    mouseMove(evt);
    
    if (hoverPiece) {
	if (pieceToPlay === 0) {
	    pieceToPlay = iToPiece(hoverPiece.x + 4 * hoverPiece.y);
	    drawMain();
	} else if (pieceToPlay !== null) {
	    fetch(`/game/${gameID}/play?player=${playerID}&body=${pieceToPlay >>> 0},${hoverPiece.x + 4 * hoverPiece.y}`, {
		method: "POST"
	    })
		.then(r => r.text())
		.then(t => {
		    if (t.startsWith("ERROR")) {
			throw t;
		    }
		    if (t.startsWith("WON")) {
			status = t;
		    }
		    pieceToPlay = null;
		    hoverPiece = {
			x: 0,
			y: 0,
		    };
		})
		.then(() => fetchBoard())
		.catch(e => console.warn(e));
	}
    } else if (inBox) {
	pieceToPlay = pieceToPlay === 0 ? null : 0;
	drawMain();
    }
}, false);

const fetchBoard = () => {
    fetch(`/game/${gameID}/board`)
	.then(r => r.text())
	.then(t => board = JSON.parse(t))
	.then(() => drawMain())
	.catch(e => console.warn(e));
}

const copyGameID = e => {
    navigator.clipboard.writeText(`http://tttt.csh.rit.edu/?join=${gameID}`);
}

const newGame = () => {
    fetch(`/new?player=${playerID}`)
	.then(r => r.text())
	.then(t => {
	    if (t.startsWith("ERROR")) {
		throw t;
	    }
	    return t;
	})
	.then(t => document.cookie = `game=${t}`)
	.then(() => startup())
	.catch(e => console.error(e));
}

const check = () => {
    if (gameID){
	if (status.startsWith("OK")) {
	    fetchBoard();
	}
	startup();
    }
    setTimeout(check, 1000);
}

const startup = () => {
    for (let cookie of document.cookie.split("; ")){
	if (cookie.split("=")[0] === "player") {
	    playerID = cookie.split("=")[1];
	} else if (cookie.split("=")[0] === "game") {
	    gameID = cookie.split("=")[1];
	}
    }

    if (!playerID) {
	playerID = Math.floor(Math.random()*10000 + 1);
	document.cookie = `player=${playerID}`;
    }

    const params = new URLSearchParams(window.location.search);
    if (params.has("join")) {
	gameID = params.get("join");
	document.cookie = `game=${gameID}`
	fetch(`/game/${gameID}/join?player=${playerID}`)
	    .then(r => r.text())
	    .then(t => {
		if (t.startsWith("ERROR")) {
		    throw t;
		}
	    })
	    .then(() => window.location.assign(window.location.toString().split("?")[0]))
	    .catch(e => console.error(e));
    }

    if (gameID) {
	fetch(`/game/${gameID}/status`)
	    .then(r => r.text())
	    .then(t => {
		if (t.startsWith("ERROR")) {
		    throw t;
		} else {
		    status = t;
		    drawMain();
		}
	    })
	    .catch(e => console.error(e));
    } else {
	drawMain();
    }
}

setTimeout(check, 1000);

startup();
