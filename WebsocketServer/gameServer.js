
const WebSocket = require('ws');

const PORT = 3002; 

class GameServer {
    constructor() {
        this.wss = new WebSocket.Server({ port: PORT });
        this.userList = new Map();
        this.userIdCounter = 0;

        this.wss.on('connection', this.handleConnection.bind(this));
    }

    handleConnection(socket) {
        const userId = this.generateUserId();
        this.userList.set(socket, userId);

        this.wss.clients.forEach((client) => {
            if (client !== socket && client.readyState === WebSocket.OPEN) {
                client.send("S'ha conectat un usuari a GAME SERVER");
            }
        });

        socket.on('message', (message) => {
            this.handleMessage(socket, message.toString());
        });

        socket.on('close', () => {
            this.handleUserDisconnect(socket);
        });
    }

    handleMessage(socket, message) {
        const json = JSON.parse(message);

        if (json.type === 'position' || json.type === 'move') {
            this.sendMessage(socket, json);
        } else {
            console.error("GAME: INCORRECT JSON TYPE");
        }
    }

    handleUserDisconnect(socket) {
        this.userList.delete(socket);
        console.log("Player disconnected from GAME SERVER");
    }    

    sendMessage(socket, message) {
        const senderUsername = this.getUsername(socket);

        for (let [client] of this.userList) {
            if (client !== socket && client.readyState === WebSocket.OPEN) {
                client.send(JSON.stringify({ type: message.type, message: message.message }));

                console.log("Sending " + JSON.stringify({ type: message.type, message: message.message }));
            }
        }
    }

    generateUserId() {
        return this.userIdCounter++;
    }

    getUsername(socket) {
        return this.userList.get(socket);
    }
}

const gameServer = new GameServer();

console.log("Game server iniciado en el puerto " + PORT);
