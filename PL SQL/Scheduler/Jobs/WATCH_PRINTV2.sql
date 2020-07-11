declare 
    req utl_http.req;
    res utl_http.resp;
    url varchar2(100) := 'http://sfcx.world-electric.co.th:4200/cinema';
    buffer varchar2(4000);
begin

	req := utl_http.begin_request(url, 'POST',' HTTP/1.1');
	utl_http.set_header(req, 'user-agent', 'mozilla/4.0'); 
	utl_http.set_header(req, 'content-type', 'application/json'); 
	utl_http.set_header(req, 'Content-Length', length(public_package.content));

	utl_http.write_text(req, public_package.content);

	res := utl_http.get_response(req);
    begin
    loop
    utl_http.read_line(res, buffer);
    --dbms_output.put_line(buffer);

    end loop;
    utl_http.end_response(res);
    exception
    when utl_http.end_of_body then
    utl_http.end_response(res);
    end; 
end;