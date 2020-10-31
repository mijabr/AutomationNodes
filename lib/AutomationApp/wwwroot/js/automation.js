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
}

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
}

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
}

const processMoveMessage = message => {
    var node = nodes[message.id];
    node.style.left = `${message.location.x}px`;
    node.style.top = `${message.location.y}px`;
    node.removeAttribute('transition-duration');

    setTimeout(() =>
        move(makeIdSelector(message.id))
            .set('left', `${message.heading.x}px`)
            .set('top', `${message.heading.y}px`)
            .ease('linear')
            .duration(message.headingEta)
            .end()
    );
}

const processRotateMessage = message => {
    var node = nodes[message.id];
    console.log(message);
    node.style.transform = `rotate(${message.rotation}deg)`;
}

const processMessage = message => {
    if (message.message === 'Create') {
        processCreateMessage(message);
    } else if (message.message === 'Move') {
        processMoveMessage(message);
    } else if (message.message === 'Rotate') {
        processRotateMessage(message);
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
