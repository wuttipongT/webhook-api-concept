create or replace TRIGGER trg_watch_print 
/*
SYSTEM NAME : SFCX
PROGRAM     : PL Trigger Watch Print

Version   Date          CREATE BY		WPSID		Detail
1.        2020-07-11    wuttipong  				    1. Initial Program
*/
FOR INSERT OR UPDATE ON sfcd_printhd
COMPOUND TRIGGER
--    BEFORE EACH ROW IS
--    BEGIN
--      null;
--    END BEFORE EACH ROW;

    AFTER EACH ROW IS
    --PRAGMA AUTONOMOUS_TRANSACTION;
    --e_credit_too_high EXCEPTION;
    --PRAGMA exception_init( e_credit_too_high, -20001 );
     jobno BINARY_INTEGER;
     datakey sfcd_printhd.SFCPR_DATAKEY%type;
    BEGIN
        IF :new.sfcpr_printsts = 'N' AND :new.sfcpr_doctype = 'RDL_LABEL' THEN
            datakey := :new.SFCPR_DATAKEY;
            --RAISE e_credit_too_high;
            --dbms_scheduler.run_job('WATCH_PRINT');
            --dbms_job.submit(jobno,'begin dbms_scheduler.run_job(''WATCH_PRINT''); end;', next_date=>SYSDATE);
            DBMS_JOB.SUBMIT(job  => jobno, what => 'SFC_SP_PRINTER(''' || datakey || ''');', next_date => sysdate); 
            --dbms_job.run('');
        END IF;
    END AFTER EACH ROW;
END;