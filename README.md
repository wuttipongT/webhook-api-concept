# Webhook คืออะไร ทำงานยังไง ?
ทุกวันนี้การติดต่อกันผ่านอินเทอร์เน็ตนั้นกลายเป็นเรื่องธรรมดาๆของหลายๆระบบ และสิ่งหนึ่งที่ทุกคนพูดถึงกันก็คือ API ที่เป็นเหมือนประตูเชื่อมต่อระบบหรือโปรแกรมต่างๆเข้าด้วยกันได้อย่างง่ายดาย แต่ว่าก็ยังมีอีกหนึ่งสิ่งที่ช่วยในการติดต่อรับส่งข้อมูลก็คือ Webhook ที่เข้ามาเป็นตัวเลือกในงานที่ API ไม่ตอบโจทย์ ซึ่งในบทความนี้เราจะมาทำความรู้จักกันว่า Webhook คืออะไร และทำงานยังไง ?

เอาแบบง่ายๆ **Webhook ก็คือการใช้งาน API แบบสลับข้างกัน** โดยปกติเวลามี “ผู้ให้บริการ” สักรายเปิด API ให้ใช้งาน เวลา “ผู้ใช้” ต้องการข้อมูลต่างๆก็จะส่ง request ไปที่ url ดังกล่าวจึงจะได้รับข้อมูลกลับมา แต่พอมาเป็น Webhook แล้วเนี่ย “ผู้ให้บริการ” จะไม่ได้มี url ใดๆมาให้เรา แต่กลับกัน “ผู้ใช้” อย่างเราๆกลับต้องมี url หรือก็คือ API ของเราเองนี่แหละส่งไปให้ผู้ให้บริการแทน

# แล้ว Webhook จะมีประโยชน์ยังไงล่ะ ?
ลองคิดตามว่าถ้ามีผู้ให้บริการสักรายหนึ่ง สมมติว่าเป็นธนาคารแล้วกัน ทางธนาคารนั้นมี API เพื่อให้บริการกับใครก็ตามที่ต้องการติดต่อรับข้อมูลต่างๆก็ทำได้สะดวก จะดึงประวัติธุรกรรมรายวัน รายเดือน ก็ทำได้ผ่าน API ทั้งหมด แต่ว่าถ้าเกิดเราอยากจะทำระบบแจ้งเตือนเมื่อมีธุรกรรมใหม่เกิดขึ้นล่ะ ถ้าเราใช้งานผ่าน API เราจะทำยังไง ?

วิธีที่ต้องทำก็คือ request ไปยัง API นั้นรัวๆ ยิ่งถี่เท่าไหร่ก็หมายความว่าจะได้รับแจ้งเตือนเร็วเท่านั้น ซึ่งมันเป็นวิธีเดียวที่จะได้ผ่าน API เมื่อเราต้องการข้อมูลแบบ “real-time” มากที่สุด ซึ่งเอาจริงๆมันก็ไม่ real-time อยู่ดี แล้วฝั่งผู้ให้บริการอย่างธนาคารก็ต้องเจอกับ request มหาศาลตลอดเวลาโดยที่ข้อมูลส่วนใหญ่ไม่มีอะไรเปลี่ยนแปลง ผู้ใช้งานก็ต้องคอยส่ง request ตลอดเวลาเช่นกัน ซึ่งมันเป็นสิ่งที่ไม่เกิดประโยชน์กับใครเลย ดังนั้น Webhook เลยเกิดมาเพื่อทำงานนี้แทน API แทนที่จะให้ผู้ใช้ส่ง request ไปหาเรื่อยๆเพราะไม่รู้ว่าเมื่อไหร่จะมีข้อมูลหรือเหตุการณ์ที่ต้องการเกิดขึ้น ก็เปลี่ยนเป็นพอมีข้อมูลหรือเหตุการณ์บางอย่างเกิดขึ้น ผู้ให้บริการก็ค่อยไปสะกิดเรียกผู้ใช้แทน เท่านี้ก็ได้ประโยชน์กันทั้งสองฝ่าย ได้ข้อมูลแบบ “real-time” อย่างแท้จริง

# Webhook ทำงานยังไง ?
อย่างที่บอกไปแล้วว่า Webhook จะเป็นการที่ผู้ให้บริการมา “สะกิด” เรียกผู้ใช้งาน ขยายความอีกหน่อยนึงก็คือ Webhook เป็นการใช้งาน API รูปแบบหนึ่งนี่แหละ ที่จะผู้ให้บริการ จะส่งข้อมูลมาให้เมื่อเกิด “เหตุการณ์” (Event) ที่ผู้ใช้ต้องการ เมื่อคนเริ่มเป็นฝั่งผู้ให้บริการก็หมายความว่าข้อมูลจะถูกส่งผ่าน Webhook แบบ real-time เลยนั่นเอง โดยส่วนมากจะส่งผ่าน HTTP POST และข้อมูลจะอยู่ในรูปแบบ JSON หรืออาจจะมีบ้างที่เป็น XML ขึ้นอยู่กับผู้ให้บริการแต่ละราย

จะใช้งาน Webhook ต้องทำอะไรบ้าง ?
1. อย่างแรกเลยต้องดูว่าผู้ให้บริการที่เราจะติดต่อด้วยเค้าให้บริการ Webhook อยู่รึเปล่า (ก็แน่ล่ะถ้าไม่ให้บริการเราจะไปใช้ได้เหรอ ?)
2. ถ้ามีการให้บริการ Webhook แล้ว ต่อมาก็คือไปไล่หาดูว่าเค้ามี “Event” อะไรบ้างที่ส่ง Webhook มาให้เราได้ เลือกหาอันที่เราต้องการซะ
3. อ่านคู่มือหรืออะไรก็ตามที่ทางผู้ให้บริการบอกเอาไว้ว่าจะส่งข้อมูลให้เราในรูปแบบไหน อย่างเช่น ส่งข้อมูลในรูปแบบ JSON เราก็ต้องดูว่า JSON หน้าตาเป็นแบบไหน แล้วตอนส่งส่งมาเป็น POST หรือ GET ก็ต้องอ่านดูให้ชัดเจน
สร้าง server ของเราเองและเตรียม API ให้พร้อมรับข้อมูลจากผู้ให้บริการ โดยจะต้องมี public url เตรียมให้ผู้ให้บริการส่งข้อมูลมาให้เรา
4. เตรียมพร้อมหมดแล้ว ก็ส่ง url ของเราไปให้ผู้ให้บริการตามช่องทางที่มีไว้ให้ แล้วก็รอ Event Webhook ทำงานเป็นอันเสร็จเรียบร้อย

# สรุปแล้ว Webhook น่าใช้มั้ย ?
หลังจากรู้จักกับ Webhook มาบ้างแล้ว ก็น่าจะเห็นถึงประโยชน์ของมัน รู้ว่า Webhook คืออะไร ทำงานยังไงกันมาบ้างแล้ว สำหรับคนที่ไม่คุ้นเคยกับ API หรือยังไม่เคยใช้งานบริการอะไรแนวๆนี้ก็อาจจะยังสับสนอยู่บ้างว่า สรุปแล้ว Webhook มันดีรึเปล่าต้องใช้แทน API มั้ย ? มาทำความเข้าใจครั้งสุดท้ายกัน แค่นี้เองแหละหลักการคร่าวๆว่าจะเลือกใช้อะไรตอนไหนดี

- API เอาไว้ใช้กรณีที่เราต้องการข้อมูลตอนที่ “เราต้องการ” 
- Webhook เอาไว้ในกรณีที่เราต้องการข้อมูลตอนที่ “มีเหตุการณ์”

อ้อยังมีอีกเรื่องที่ต้องระวังหรือวางแผนตอนจะใช้งาน Webhook ก็คือเรื่องความปลอดภัย เช่นเดียวกับ API ทั่วไปที่บางครั้งก็ต้องมีการยืนยันตัวตนคนที่จะใช้งานได้ ในด้าน API ที่ server ของเราก็อาจจะต้องจำกัดให้มีเฉพาะผู้ให้บริการ Webhook เท่านั้นถึงจะมาใช้งานได้ ก็ต้องไปศึกษาวิธีทำให้ปลอดภัยกันเพิ่มเติมดู เช่น อาจจะใช้ token หรือทำ Basic Auth ก็ว่ากันไป

อาจจะดูเหมือนว่าการใช้งาน Webhook จะมีอะไรให้คิดให้ทำอยู่หลายอย่าง ดังนั้นวิธีที่ดีที่สุดในการหัดใช้ Webhook ก็คือการลงมือลองทำจริงๆนั่นเอง ลองหาเครื่องมือง่ายๆไว้จำลองเป็น server ของเราก็ได้อย่างเช่น https://webhook.site/ หรือ https://ngrok.com/ ก็ทำให้เรามี public url ไว้ทดสอบกับ Webhook แล้ว ขอให้สนุกกับ Webhook ครับ

# ขอบคุณ
I love content the **[borntoDev](https://www.borntodev.com/2020/07/01/webhook-%E0%B8%84%E0%B8%B7%E0%B8%AD%E0%B8%AD%E0%B8%B0%E0%B9%84%E0%B8%A3-%E0%B8%97%E0%B8%B3%E0%B8%87%E0%B8%B2%E0%B8%99%E0%B8%A2%E0%B8%B1%E0%B8%87%E0%B9%84%E0%B8%87/)**.