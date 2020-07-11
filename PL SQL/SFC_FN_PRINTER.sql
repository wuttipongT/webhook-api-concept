create or replace FUNCTION SFC_FN_PRINTER (DATAKEY IN SFCD_PRINTHD.SFCPR_DATAKEY%type)
/*
SYSTEM NAME : SFCX
PROGRAM     : PL Function PRINTER

Version   Date          CREATE BY		WPSID		Detail
1.        2020-07-11    wuttipong  				    1. Initial Program
*/
RETURN varchar2
IS
TYPE v_typ_rec IS record (
		DOCUMENTID SFCD_PRINTHD.SFCPR_DOCID%type,
		DOCUMENTNAME SFCD_PRINTHD.SFCPR_DOCNAME%type,
		PRINTSERVERID SFCD_PRINTHD.SFCPR_PRINTERID%type,
		PRINTERNAME SFCC_PNTMST.SFCPN_PRINTERNAME%type,
		SENDER SFCD_PRINTHD.SFCPR_REGUSR%type,
		PRINTSTATUS SFCD_PRINTHD.SFCPR_PRINTSTS%type,
		DOCUMENTTYPE SFCD_PRINTHD.SFCPR_DOCTYPE%type,
		DOCUMENTKEY SFCD_PRINTHD.SFCPR_DATAKEY%type,
		FILEPATH SFCC_PNTTEMPLATE.SFCTP_FILENAME%type
	);
TYPE v_typ_table IS TABLE OF v_typ_rec;
o v_typ_rec;
vcontent varchar2(4000);
jobs varchar2(4000);
vconf sfcc_config%rowtype;
BEGIN
	begin
		SELECT * INTO  vconf FROM sfcc_config WHERE sfcfg_cat='BNT' AND sfcfg_type = 'FILEPATH' AND sfcfg_key = 'TEMPLATE';
	EXCEPTION WHEN no_data_found THEN 
		RETURN '{"success":"false","message":"Bartender Template config fail!!!!!"}';
	END;

    begin
        SELECT 
            hd.SFCPR_DOCID,
            hd.SFCPR_DOCNAME,
            hd.SFCPR_PRINTERID,
            mt.SFCPN_PRINTERNAME,
            hd.SFCPR_REGUSR,
            hd.SFCPR_PRINTSTS,
            hd.SFCPR_DOCTYPE,
            hd.SFCPR_DATAKEY,
            tm.SFCTP_FILENAME
            into o
        FROM SFCD_PRINTHD hd
        LEFT JOIN SFCC_PNTTEMPLATE tm ON tm.SFCTP_DOCNAME = hd.SFCPR_DOCNAME AND tm.SFCTP_STATUS = 'Y'
        LEFT JOIN SFCC_PNTMST mt ON mt.SFCPN_PRINTERID = hd.SFCPR_PRINTERID 
        WHERE hd.SFCPR_DATAKEY = DATAKEY and hd.SFCPR_PRINTSTS = 'N' AND rownum <=1;
    exception when no_data_found then
        RETURN '{"success":"false","message":"No Job."}';
	end;

	jobs := '"DocumentID":"'|| o.DOCUMENTID ||'","DocumentName":"'|| o.DOCUMENTNAME ||'","PrintServerID":"'|| o.PRINTSERVERID ||'","PrinterName":"'|| o.PRINTERNAME ||'","Sender":"'|| o.SENDER ||'","PrintStatus":"false","DocumentType":"'|| o.DOCUMENTTYPE ||'","DocumentKey":"'|| o.DOCUMENTKEY ||'","FilePath":"'|| REPLACE(concat(vconf.SFCFG_VALUE, '\' || o.FILEPATH), '\', '\\') ||'"';

	DECLARE
        attrs varchar2(4000) := '[';
	begin
		FOR t IN (
			SELECT * FROM SFCD_PRINTDET WHERE SFCPR_DOCID = o.DOCUMENTID
		) LOOP
			attrs := attrs || '{"Parameter":"'|| t.SFCPR_ATTR ||'","Value":"'|| t.SFCPR_VALUE ||'"},';
		END LOOP;
		attrs := RTRIM(attrs, ',') || ']';
		jobs := jobs || ',"DocumentDatas":' || attrs;
	END;

	vcontent := '{"success":"true","message":"","JOBS":[{'|| jobs ||'}]}';
	--dbms_output.put_line(content);
	RETURN vcontent; 
END;