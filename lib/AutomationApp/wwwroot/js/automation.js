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

const setUpNode = (node, nodeInfo) => {
    node.setAttributeNS(null, 'id', makeId(nodeInfo.id));
    node.setAttributeNS(null, 'class', 'node');
    node.setAttributeNS(null, 'src', `assets/${nodeInfo.image}`)
};

const addToWorld = (node, nodeInfo) => {
    document.getElementById("world").appendChild(node);
    nodes[nodeInfo.id] = node;
};

const createImg = nodeInfo => {
    const imgElem = document.createElement('img');
    setUpNode(imgElem, nodeInfo);
    addToWorld(imgElem, nodeInfo);
    return imgElem;
};

const getOrCreateNode = nodeInfo => {
    var node = nodes[nodeInfo.id];
    if (node == null) {
        node = createImg(nodeInfo);
    }
    return node;
};

const processMessage = message => {
    message.forEach(nodeInfo => {
        var node = getOrCreateNode(nodeInfo);

        node.style.left = `${nodeInfo.location.x}px`;
        node.style.top = `${nodeInfo.location.y}px`;
        node.removeAttribute('transition-duration');

        setTimeout(() => {
            move(makeIdSelector(nodeInfo.id))
                .set('left', `${nodeInfo.heading.x}px`)
                .set('top', `${nodeInfo.heading.y}px`)
                .duration(nodeInfo.headingEta)
                .end();
        });
    });
}

setInterval(() => {
    if (isPageLoaded && messageQueue.length > messageQueueIndex) {
        processMessage(messageQueue[messageQueueIndex++]);
    }
}, 20);

connection.on("AutomationMessage", message => messageQueue.push(message));

connection.start().then(() => {
    console.log('started');
}).catch(err => console.error(err.toString()));




//const createDiv = id => {
//    var divElem = document.createElement("div");
//    divElem.textContent = "O";
//    setUpNode(divElem);
//    addToWorld(divElem);
//    return divElem;
//}

//const createSvg = id => {
//    var xmlns = "http://www.w3.org/2000/svg";
//    var svgElem = document.createElementNS(xmlns, "svg");
//    svgElem.setAttributeNS(null, "viewBox", "0 0 1000 1000");

//    var g = document.createElementNS(xmlns, "g");
//    svgElem.appendChild(g);
//    var path = document.createElementNS(xmlns, "path");
//    path.setAttributeNS(null, 'd', 'M500,10C229.4,10,10,229.4,10,500c0,270.6,219.4,490,490,490c270.6,0,490-219.4,490-490C990,229.4,770.6,10,500,10z M815,815c-40.9,40.9-88.6,73.1-141.6,95.5c-54.9,23.2-113.2,35-173.4,35c-60.2,0-118.5-11.8-173.4-35C273.6,888,225.9,855.9,185,815s-73-88.6-95.5-141.6c-23.2-54.9-35-113.2-35-173.4c0-60.2,11.8-118.5,35-173.4c22.4-53,54.6-100.7,95.5-141.6s88.6-73,141.6-95.5c54.9-23.2,113.2-35,173.4-35c60.2,0,118.5,11.8,173.4,35c53,22.4,100.7,54.6,141.6,95.5c40.9,40.9,73,88.6,95.5,141.6c23.2,54.9,35,113.2,35,173.4c0,60.2-11.8,118.5-35,173.4C888,726.4,855.9,774.1,815,815z');
//    g.appendChild(path);

//    setUpNode(svgElem);
//    addToWorld(svgElem);

//    return svgElem;
//}
