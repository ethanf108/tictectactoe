FROM debian:latest

# Install gforth and needed packages.
# Why is it so hard to make `apt` quiet!!!
RUN DEBIAN_FRONTEND=noninteractive apt -qq update && apt -qqy install libtool libtool-bin gforth git >/dev/null

# Set up forth libraries
WORKDIR /forth-libs
RUN git clone --depth 1 https://github.com/irdvo/ffl && cd ffl/ && cp -r ffl/ /usr/share/gforth/*.*.*/
RUN git clone --depth 1 https://github.com/urlysses/1991

WORKDIR /app
RUN cp /forth-libs/1991/1991.fs .
COPY . .

# STUPID OKD
RUN mkdir /.gforth && chmod 777 /.gforth && chmod +t /.gforth

CMD gforth main.fs
