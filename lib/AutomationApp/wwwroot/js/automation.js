"use strict";

var isPageLoaded = false;
var connection = new signalR.HubConnectionBuilder().withUrl("/automationHub").build();
var messageQueue = [];
var messageQueueIndex = 0;
var nodes = {};

this.onLoad = () => {
    isPageLoaded = true;
}

const makeId = id => `id-${id}`;

const makeIdSelector = id => `#${makeId(id)}`;

const setUpNode = (element, message) => {
    element.setAttributeNS(null, 'id', makeId(message.id));
    element.setAttributeNS(null, 'class', 'node');
};

const createDiv = message => {
    const divElem = document.createElement('div');
    setUpNode(divElem, message);
    return divElem;
};

const createImg = message => {
    const imgElem = document.createElement('img');
    setUpNode(imgElem, message);
    imgElem.setAttributeNS(null, 'src', `assets/${message.image}`)
    return imgElem;
};

const getParentElement = parentId => {
    if (parentId) {
        return nodes[parentId];
    } else {
        return document.getElementById("world")
    }
};

const getOrCreateNode = message => {
    var node = nodes[message.id];
    if (node == null) {
        if (message.type === 'Div') {
            node = createDiv(message);
            if (message.innerHtml) {
                node.innerHTML = message.innerHtml;
            }
        } else if (message.type === 'Img') {
            node = createImg(message);
        }

        getParentElement(message.parentId).appendChild(node);
        nodes[message.id] = node;
    }
    return node;
};

const processCreateMessage = message => {
    getOrCreateNode(message);
};

const processSetPropertyMessage = message => {
    nodes[message.id].style.setProperty(message.name, message.value);
};

const processTransitionMessage = message => {
    const node = nodes[message.id];
    node.removeAttribute('transition-duration');

    const moveNode = move(node);
    for (var property in message.properties) {
        moveNode.set(property, message.properties[property]);
    }

    setTimeout(() => {
        moveNode.duration(message.duration)
            .ease('linear')
            .end();
    }, 20);
};

const processMessage = message => {
    if (message.message === 'SetProperty') {
        processSetPropertyMessage(message);
    } else if (message.message === 'SetTransition') {
        processTransitionMessage(message);
    } else if (message.message === 'Create') {
        processCreateMessage(message);
    } else if (message.message === 'World') {
        nodes[message.id] = document.getElementById("world");
    }
};

const processMessages = messages => {
    messages.forEach(message => processMessage(message));
};

setInterval(() => {
    if (isPageLoaded && messageQueue.length > messageQueueIndex) {
        processMessages(messageQueue[messageQueueIndex++]);
    }
}, 20);

connection.on("AutomationMessage", message => messageQueue.push(message));

connection.start().then(() => {
    console.log('hub connected');
}).catch(err => console.error(err.toString()));
