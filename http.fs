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

\ parse to end of line
: >eol ( "input to newline" -- s-addr u )  10 parse ;

\ Parsing methods TODO: implement POST HEAD OPTIONS PUT DELETE etc.
: GET 
	parse-name to path:length to path
	parse-name to version:length to version ;

: User-Agent:
	>eol to user-agent:length to user-agent ;

: Host:
	parse-name to host:length to host ;

: Accept:
	>eol to accept:length to accept ;

: Accept-Language:
	>eol to accept-language:length to accept-language ;

: Accept-Encoding:
	>eol to accept-encoding:length to accept-encoding ;

: Connection:
	parse-name to connection:length to connection ;

: Cache-Control:
	>eol to cache-control:length to cache-control ;

: Referer:
	>eol to referer:length to referer ;

\ Routing code
: /. 	\ this is the default path
	s\" HTTP/1.1 200 OK\r\nContent-Length: 13\r\n\r\nHello World!\n" ;

: /foo	\ this is a sample route
	s\" HTTP/1.1 200 OK\r\nContent-Length: 13\r\n\r\n{\"foo\":\"bar\"}" ;

: http/404	\ this is an error page
	s\" HTTP/1.1 404 Not Found\r\nContent-Length: 10\r\n\r\nNot Found\n" ;

create route 4096 allot	\ path must be smaller than 4k because we only read 4k
\ path to route
: path>route 
	s" /" path path:length compare 0= if  
		2 route c! 
		[char] / route 1+ c! 
		[char] . route 2 + c! 
	else
		path:length route c! 
		path route 1+ path:length cmove 
	then ;


\ tests if a route exists
: route? ( -- true|false)
	route find nip ;

\ dispatch
: dispatch route count evaluate ;

: send-response
	dup to response:length 		\ set the response length
	response swap cmove		\ copy repond to response
	response response:length client write-socket
	client close-socket ;

\ web server
: serve begin
	server 255 listen
	server accept-socket to client
	client request size read-socket to request:length drop
	request request:length evaluate
	path>route route? if dispatch else http/404 then
	send-response
again ;

serve
