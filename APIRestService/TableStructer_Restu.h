
TABLE=ORDER_HEAD
FIELD=OrderNo,			TYPE=varchar,SIZE=60 //TableID-TimeTick
FIELD=TableID,			TYPE=varchar,SIZE=20
FIELD=OrderDateTime,	TYPE=datetime
FIELD=OrderByUserID,	TYPE=varchar,SIZE=20
FIELD=CxlDateTime,		TYPE=datetime
FIELD=CxlUserID,		TYPE=varchar, SIZE = 20
FIELD=CookAckDateTime,	TYPE=datetime
FIELD=CookAckUserID,	TYPE=varchar,SIZE=20
FIELD=ReceiveDateTime,	TYPE=datetime
FIELD=ReceiveUserID,	TYPE=varchar,SIZE=20
PRIMARYKEY=OrderNo

TABLE=ORDER_DETAIL
FIELD=OrderNo,			TYPE=varchar,SIZE= 60 //TableID-TimeTick
FIELD=Suffix,			TYPE=int
FIELD=Qty,				TYPE=int
FIELD=MenuID,			TYPE=varchar, SIZE=30
FIELD=MenuMemo,			TYPE=varchar, SIZE=300
FIELD=CookAckDateTime,	TYPE=datetime
FIELD=CookAckUserID,	TYPE=varchar, SIZE=20 
FIELD=EntryDateTime,		TYPE=datetime
FIELD=FinishCookDateTime,	TYPE=datetime
FIELD=FinishCookUserID,		TYPE=varchar, SIZE=20
FIELD=ServeCookDateTime,	TYPE=datetime
FIELD=ServeCookUserID,		TYPE=varchar, SIZE=20 
FIELD=CxlDateTime,		TYPE=datetime
FIELD=CxlUserID,		TYPE=varchar, SIZE=20
FIELD=ChargeAmt,		TYPE=float
PRIMARYKEY=OrderNo,Suffix

TABLE=RECEIVE_HEAD
FIELD=RecevieNo,		TYPE=varchar, SIZE=60 //R+TableID-TimeTick
FIELD=TableID,			TYPE=varchar, SIZE=20
FIELD=CxlDateTime,		TYPE=datetime
FIELD=CxlUserID,		TYPE=varchar, SIZE=20
FIELD=ReceiveDateTime,	TYPE=datetime
FIELD=ReceiveUserID,	TYPE=varchar, SIZE=20
FIELD=ChargeAmt,		TYPE=float
FIELD=DiscountAmt,		TYPE=float
FIELD=TotalAmt,			TYPE=float
PRIMARYKEY=RecevieNo

TABLE=RECEIVE_DETAIL
FIELD=RecevieNo,		TYPE=varchar, SIZE=60 //TableID + TimeTick
FIELD=Suffix,			TYPE=int
FIELD=OrderNo,			TYPE=varchar, SIZE=30
FIELD=ChargeAmt,		TYPE=float
PRIMARYKEY=RecevieNo,Suffix
 