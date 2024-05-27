
const WebSocket = require('ws');

const PORT = 3001;

class ChatServer {
    constructor() {
        this.wss = new WebSocket.Server({ port: PORT });
        this.userList = new Map();

        this.wss.on('connection', this.handleConnection.bind(this));
    }

    handleConnection(socket) {
        this.userList.set(socket, "");

        this.wss.clients.forEach((client) => {
            if (client !== socket && client.readyState === WebSocket.OPEN) {
                client.send("S'ha conectat un usuari");
            }
        });

        socket.on('message', (message) => {
            this.handleMessage(socket, message.toString());
        });

        socket.on('close', () => {
            this.handleUserDisconnect(socket);
        });
    }

    handleUserDisconnect(socket) {
        const username = this.getUsername(socket);

        this.userList.delete(socket);
        console.log("S'ha desconectat l'usuari " + username);
    }

    handleMessage(socket, message) {
        const json = JSON.parse(message);
        const username = this.getUsername(socket);

        if (json.type === 'chat') {
            this.sendMessage(socket, json);
        } else if (json.type === 'username') {
            this.setUsername(socket, json.message);
        } else {
            console.error("INCORRECT JSON TYPE");
        }
    }

    sendMessage(socket, message) {
        var senderUsername = this.getUsername(socket);
    
        for (let [client, userName] of this.userList) {
            if (client !== socket && client.readyState === WebSocket.OPEN) {
                client.send(JSON.stringify({ type: `${message.type}`, username: `${senderUsername}`, message: `${message.message}`}));
                console.log("Sending "+ JSON.stringify({ type: `${message.type}`, username: `${senderUsername}`, message: `${message.message}`}));
            }
        }
    }

    getUsername(socket) {
        return this.userList.get(socket);
    }

    setUsername(socket, username) {
        this.userList.set(socket, username);
        console.log("Usuari " + username + " conectat.");
    }
}

const chatServer = new ChatServer()

console.log("Chat server iniciado en el puerto " + PORT);
