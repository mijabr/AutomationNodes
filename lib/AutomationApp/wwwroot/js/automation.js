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

const setUpNode = (element, nodeInfo) => {
    element.setAttributeNS(null, 'id', makeId(nodeInfo.id));
    element.setAttributeNS(null, 'class', 'node');
};

const createDiv = nodeInfo => {
    const divElem = document.createElement('div');
    setUpNode(divElem, nodeInfo);
    return divElem;
}

const createImg = nodeInfo => {
    const imgElem = document.createElement('img');
    setUpNode(imgElem, nodeInfo);
    imgElem.setAttributeNS(null, 'src', `assets/${nodeInfo.image}`)
    return imgElem;
};

const getOrCreateNode = (nodeInfo, parentElement) => {
    var node = nodes[nodeInfo.id];
    if (node == null) {
        if (nodeInfo.type === 'Div') {
            node = createDiv(nodeInfo);
        } else if (nodeInfo.type === 'Img') {
            node = createImg(nodeInfo);
        }
        parentElement.appendChild(node);
        nodes[nodeInfo.id] = node;
    }
    return node;
};

const processNode = (nodeInfo, parentElement) => {
    var node = getOrCreateNode(nodeInfo, parentElement);
    node.style.left = `${nodeInfo.location.x}px`;
    node.style.top = `${nodeInfo.location.y}px`;
    node.style.transform = `rotate(${nodeInfo.rotation}deg)`;
    node.removeAttribute('transition-duration');

    setTimeout(() =>
        move(makeIdSelector(nodeInfo.id))
            .set('left', `${nodeInfo.heading.x}px`)
            .set('top', `${nodeInfo.heading.y}px`)
            .ease('linear')
            .duration(nodeInfo.headingEta)
            .end()
    );

    processNodes(nodeInfo.children, node);
};

const processNodes = (nodeInfos, parentElement) => nodeInfos.forEach(nodeInfo => processNode(nodeInfo, parentElement));

const processMessage = message => processNodes(message, document.getElementById("world"));

setInterval(() => {
    if (isPageLoaded && messageQueue.length > messageQueueIndex) {
        processMessage(messageQueue[messageQueueIndex++]);
    }
}, 20);

connection.on("AutomationMessage", message => messageQueue.push(message));

connection.start().then(() => {
    console.log('started');
}).catch(err => console.error(err.toString()));
