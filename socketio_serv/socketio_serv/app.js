'use strict';

var express = require('express');
var app = express();
var server = require('http').createServer(app);
var io = require('socket.io')(server);
var ip = require("ip");
var connectedUserMap = new Map();
const fetch = require('node-fetch');
const client = require('socket.io-client');
var bodyParser = require('body-parser');

app.use(express.static(__dirname + '/node_modules'));
app.use(bodyParser());

app.use(function (req, res, next) {
    res.header("Access-Control-Allow-Origin", "*"); // update to match the domain you will make the request from
    res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
    next();
});

app.get('/', function (req, res, next) {
    res.sendFile(__dirname + '/index.html');
});

app.get('/ip', function (req, res, next) {
    return res.send({ ip: ip.address()});
});

app.get('/info', function (req, res, next) {
    
    io.clients((error, clients) => {
        if (error) throw error;
        //console.log(clients); // => [6em3d4TJP8Et9EMNAAAA, G5p55dHhGgUnLUctAAAB]
        res.set({ 'content-type': 'application/json; charset=utf-8' });
        return res.json({ client: clients, users: [...connectedUserMap.values()] });
    });
});

app.get('/users', function (req, res, next) {

    return res.json([...connectedUserMap.values()].filter(o => ('ip' in o)));
});

app.get('/printers', function (req, res, next) {

    return res.json([...connectedUserMap.values()].filter(o => o.hasOwnProperty('printers')));
});

app.get('/watch', function (req, res, next) {

    let socket = client('http://sfcx.world-electric.co.th:4200/');
    socket.on('connect', function () {
        setTimeout(function () {
            fetch('http://sfcx.world-electric.co.th/API/PRINTER?REQ=GET&IP=' + req.query.ip)
                .then(res => res.json())
                .then(data => {

                    socket.emit('printClientMessage', data);
                    socket.disconnect();

                });

        }, 1100);

        console.log('Connected.');
    });

    console.log('watching...');
    return res.json(['watching...']);
});

app.post('/cinema', function (req, res, next) {
    let socket = client('http://localhost:4200/');
    let data = req.body;
    socket.on('connect', function () {
        socket.emit('printClientMessage', data);
        socket.disconnect();
        console.log('Connected.');
    });

    return res.json([req.query.room, req.query.partySize]);
});
server.listen(4200);

io.on('connection', function (socket) {

    let connectedUserId = socket.id;
    //add property value when assigning user to map
    connectedUserMap.set(socket.id, { id: socket.id, status: 'online', time: Date.now() });

    //sets the user name for the user.
    socket.on('recieveUserName', function (data) {
        //find user by there socket in the map the update name property of value
        let d = typeof data === 'string' || data instanceof String ? JSON.parse(data) : data;
        let user = connectedUserMap.get(connectedUserId);
        //user.name = d.name;
        //user.ip = d.ip;
        //user.message = `connected.`;
        Object.assign(user, d);
        //io.emit('message', user);
        io.emit('refresh');
        io.emit('log', user);
    });

    io.emit('refresh');

    socket.on('disconnect', function () {
        //get access to the user currently being used via map.
        //let user = connectedUserMap.get(connectedUserId);
        //user.status = 'offline';
       /* let user = connectedUserMap.get(connectedUserId);
        user.message = 'disconnected.';
        user.status = 'destroy';
        user.time = Date.now();
        io.emit('message', user);
        */
        connectedUserMap.delete(connectedUserId);
        io.emit('refresh');
        
    });

    socket.on('someEventForAEveryone', function (data) {
        //data.msg - message to be output
        let user = connectedUserMap.get(connectedUserId);
	    let d = {...user, ...data };
        io.emit('message', d);
    });
    
    //example of sending another user a message
    //https://gist.github.com/alexpchin/3f257d0bb813e2c8c476 - reference
    socket.on('sendOtherClientMessage', function (data) {
        //data.id - the user to emit to.
       
        let userId = undefined;
        for (let [id, o] of connectedUserMap) {
            if (o.ip === data.ip) {
                userId = id;
                break;
            }
        }

        let user = connectedUserMap.get(connectedUserId);
	    let d = {...user, ...data }; 
        if (connectedUserMap.has(userId)) {
            let message = data.message ? data.message : 'for your eyes only';

            //user.to = data.ip;
            //user.message = message;
            socket.broadcast.to(userId).emit('message', d);
            
        } else {
            //socket.broadcast.to(data.id).emit('message', "Client is currently not connected.");
            console.log("Client is currently not connected.");
            d.message = 'Client is currently not connected.';
        }

        io.emit('log', d);
    });

    socket.on('printClientMessage', function (data) {
        //data.id - the user to emit to.

        let userId = undefined;
        for (let [id, o] of connectedUserMap) {
            let printerId = data.JOBS.length > 0 ? data.JOBS[0]['PrintServerID'] : null;
            if (o.ip === printerId && o.hasOwnProperty('printers')) {
                userId = id;
                break;
            }
        }

        let user = connectedUserMap.get(connectedUserId);
        let d = { ...user, ...data }; 
        if (connectedUserMap.has(userId)) {
            d.to = data.ip;
            d.message = 'printed.';
            socket.broadcast.to(userId).emit('print', d);

        } else {
            //socket.broadcast.to(data.id).emit('message', "Client is currently not connected.");
            console.log("Client is currently not connected.");
            d.message = 'Client Printer is currently not connected.';
        }

        io.emit('log', d);
    });
    
});
console.log('Server running at http://sfcx.world-electric.co.th:4200/');
