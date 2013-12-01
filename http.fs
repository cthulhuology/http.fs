#! /usr/bin/env gforth
\ sample http server
\ © 2013 David J. Goehrig <dave@dloh.org>

require unix/socket.fs

4096 constant size		\ 1 page buffer sizes
create request size allot	\ create a big buffer for http requests
0 value request:length
create response size allot	\ create a big response buffer for http responses
0 value response:length

\ Configure the port we're listening on.
8080 create-server value server
." Server running on socket " server . cr

\ The socket the client will use
0 value client

\ HTTP parameters
0 value path:length
0 value path

0 value version:length
0 value version

0 value user-agent:length
0 value user-agent

0 value host:length
0 value host

0 value accept:length
0 value accept			\ overrides a built in but we don't need it :)

0 value accept-language:length
0 value accept-language

0 value accept-encoding:length
0 value accept-encoding

0 value connection:length
0 value connection

0 value cache-control:length
0 value cache-control

0 value referer:length
0 value referer


\ Parsing methods TODO: implement POST HEAD OPTIONS PUT DELETE etc.
: GET ." get method "  cr
	parse-name to path:length to path
	parse-name to version:length to version ;

: User-Agent: ." got user agent " cr
	10 parse to user-agent:length to user-agent ;

: Host: ." got host " cr
	parse-name to host:length to host ;

: Accept: ." got accept" cr
	10 parse to accept:length to accept ;

: Accept-Language: ." got accept-language" cr
	10 parse to accept-language:length to accept-language ;

: Accept-Encoding: ." got accept-encoding" cr
	10 parse to accept-encoding:length to accept-encoding ;

: Connection: ." got connection" cr
	parse-name to connection:length to connection ;

: Cache-Control: ." got cache-control" cr
	10 parse to cache-control:length to cache-control ;

: Referer: ." got referer" cr
	10 parse to referer:length to referer ;

\ Routing code
: /. 	\ this is the default path
	s\" HTTP/1.1 200 OK\r\nContent-Length: 13\r\n\r\nHello World!\n" ;

: /foo	\ this is a sample route
	s\" HTTP/1.1 200 OK\r\nContent-Length: 13\r\n\r\n{\"foo\":\"bar\"}" ;

: http/404	\ this is an error page
	s\" HTTP/1.1 404 Not Found\r\nContent-Length: 10\r\n\r\nNot Found\n" ;

create command 4096 allot	\ path must be smaller than 4k because we only read 4k

\ web server
: serve begin
	." serving " cr
	server 255 listen
	server accept-socket to client
	." got client " client . cr
	." accepted a socket " cr
	client 0< invert if
		client request size read-socket to request:length drop
		request request:length type
		request request:length evaluate
		path path:length dup command c! command 1+ swap cmove
		." command is " command count type cr
		command find 0= if ." HTTP 404  not found " cr
			drop http/404 
		else drop
			\ serve the index path if path is / otherwise route
			s" /" path path:length compare 0= if  /. else
				path path:length evaluate	( -- c-addr u )
			then
		then
		dup to response:length 		\ set the response length
		response swap cmove		\ copy repond to response
		response response:length client write-socket
		client close-socket
	then
again 
;

serve
