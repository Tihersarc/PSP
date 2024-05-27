const { spawn } = require('child_process');
const { type } = require('os');
const WebSocket = require('ws');
const PORT = 3000;
const PASSWORD = "123";

let isAdmin;

let chatServerProcess;
let gameServerProcess;
let storeServerProcess;
let userList = new Map();

const wss = new WebSocket.Server({port:PORT});

wss.on('listening', () => { 
	console.log("WebSocket server is listening on port " + PORT); 
}); 

wss.on('error', (error) => { 
	console.error(`WebSocket server error: ${error.message}`); 
});

wss.on('connection', (socket) => {
    userList.set(socket, "");

    wss.clients.forEach((client) => {
        if (client !== socket && client.readyState === WebSocket.OPEN) {
            client.send("S'ha conectat un usuari");
        }
    });

    socket.on('message', (message) => {
        handleMessage(socket, message.toString());
    })

    socket.on('close', () => {
        handleUserDisconnect(socket);
    });
});

function handleUserDisconnect(socket) {
    //const username = this.getUsername(socket);

    userList.delete(socket);
    console.log("S'ha desconectat l'usuari ");
}

function handleMessage(socket, message) {
    const json = JSON.parse(message);

    if (json.type === 'server' && isAdmin) {
        if (json.message === 'chat') {
            toggleChatServer();
        } 
        else if (json.message === 'game') {
            toggleGameServer();
        } 
        else if (json.message === 'store') {
            toggleStoreServer();
        } 
        else {
            console.warn("Incorrect JSON message");
        }
    }
    else if (json.type === 'login') {
        console.log("json: " + typeof json.message + "pass: " + typeof PASSWORD)
        if (json.message == PASSWORD) {
            isAdmin = true;
            socket.send(JSON.stringify({ type: `${json.type}`, message: `${isAdmin}`}))
            console.log(JSON.stringify({ type: `${json.type}`, message: `${isAdmin}`}))
            console.log("User logged in as Admin")
        }
        else {
            isAdmin = false;
            socket.send(JSON.stringify({ type: `${json.type}`, message: `${isAdmin}`}))
            console.log("User failed to log in as Admin")
        }
    } 
    else {
        console.warn("INCORRECT JSON TYPE");
    }
}

function startServers() {
    startChatServer();
    startGameServer();
    startStoreServer();

    console.log("Servidores iniciados.");
}

function stopServers() {
    if (chatServerProcess) {
        chatServerProcess.kill();
    }
    if (gameServerProcess) {
        gameServerProcess.kill();
    }
    if (storeServerProcess) {
        storeServerProcess.kill();
    }

    console.log("Servidores detenidos.");
}

function startChatServer() {
    if (!chatServerProcess) {
        chatServerProcess = spawn('node', ['./chatServer.js']);

        chatServerProcess.stdout.on('data', (data) => {
            console.log(`Chat server stdout: ${data}`);
        });

        chatServerProcess.stderr.on('data', (data) => {
            console.error(`Chat server stderr: ${data}`);
        });

        chatServerProcess.on('close', (code) => {
            console.log(`Chat server process exited with code ${code}`);
            chatServerProcess = null;
        });

        console.log("Servidor de chat iniciado.");
    } else {
        console.log("El servidor de chat ya está en ejecución.");
    }
}

function toggleChatServer() {
    if (chatServerProcess) {
        chatServerProcess.kill();
        console.log("Servidor de chat detenido.");
    } else {
        console.log("El servidor de chat no está en ejecución.");
        startChatServer();
    }
}

function startGameServer() {
    if (!gameServerProcess) {
        gameServerProcess = spawn('node', ['./gameServer.js']);

        gameServerProcess.stdout.on('data', (data) => {
            console.log(`Game server stdout: ${data}`);
        });

        gameServerProcess.stderr.on('data', (data) => {
            console.error(`Game server stderr: ${data}`);
        });

        gameServerProcess.on('close', (code) => {
            console.log(`Game server process exited with code ${code}`);
            gameServerProcess = null;
        });

        console.log("Servidor de juego iniciado.");
    } else {
        console.log("El servidor de juego ya está en ejecución.");
    }
}

function toggleGameServer() {
    if (gameServerProcess) {
        gameServerProcess.kill();
        console.log("Servidor de juego detenido.");
    } else {
        console.log("El servidor de juego no está en ejecución.");
        startGameServer();
    }
}

function startStoreServer() {
    if (!storeServerProcess) {
        storeServerProcess = spawn('node', ['./storeServer.js']);

        storeServerProcess.stdout.on('data', (data) => {
            console.log(`Store server stdout: ${data}`);
        });

        storeServerProcess.stderr.on('data', (data) => {
            console.error(`Store server stderr: ${data}`);
        });

        storeServerProcess.on('close', (code) => {
            console.log(`Store server process exited with code ${code}`);
            storeServerProcess = null;
        });

        console.log("Servidor de tienda iniciado.");
    } else {
        console.log("El servidor de tienda ya está en ejecución.");
    }
}

function toggleStoreServer() {
    if (storeServerProcess) {
        storeServerProcess.kill();
        console.log("Servidor de tienda detenido.");
    } else {
        console.log("El servidor de tienda no está en ejecución.");
        startStoreServer();
    }
}

startServers();
