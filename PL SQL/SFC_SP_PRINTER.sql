create or replace PROCEDURE SFC_SP_PRINTER(datakey in SFCD_PRINTHD.SFCPR_DATAKEY%type)
/*
SYSTEM NAME : SFCX
PROGRAM     : PL Procedure PRINTER

Version   Date          CREATE BY		WPSID		Detail
1.        2020-07-11    wuttipong  				    1. Initial Program
*/
AS
 content varchar2(4000) := '';
BEGIN
    content := sfc_fn_printer(datakey);
    --insert into sfctm_preprintj (sfcjson_text) values (content);
	--dbms_output.put_line(content);
    public_package.content := content;
    dbms_scheduler.run_job('WATCH_PRINTV2');
    update sfcd_printhd set SFCPR_PRINTSTS = 'Y' where SFCPR_DATAKEY = datakey;
END;