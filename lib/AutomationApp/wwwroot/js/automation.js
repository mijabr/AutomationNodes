"use strict";

var isPageLoaded = false;
var connection = new signalR.HubConnectionBuilder().withUrl("/automationHub").build();
var messageQueue = [];
var messageQueueIndex = 0;
var nodes = {};
var keyframesStylesheet;

const sendCaps = () => {
    const caps = {
        windowInnerWidth: window.innerWidth,
        windowInnerHeight: window.innerHeight,
        userAgent: navigator.userAgent
    };
    connection.invoke("SendCapabilities", caps);
};

const onClick = message => connection.invoke("SendMessage", message);

this.onLoad = () => {
    isPageLoaded = true;
    keyframesStylesheet = document.createElement('style');
    document.head.appendChild(keyframesStylesheet);
    document.body.addEventListener('click', () => onClick('body'));
};

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
    imgElem.setAttributeNS(null, 'src', message.imageName);
    return imgElem;
};

const getParentElement = parentId => {
    if (parentId) {
        return nodes[parentId];
    } else {
        return document.getElementById("world");
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

const processCreateMessage = message => getOrCreateNode(message);

const processSetPropertyMessage = message => {
    if (message.value.trim() === '') {
        nodes[message.id].style.removeProperty(message.name);
    } else {
        if (message.name === 'onclick') {
            nodes[message.id].addEventListener('click', (e) => { e.stopPropagation(); onClick(message.value); });
        } else {
            nodes[message.id].style.setProperty(message.name, message.value);
        }
    }
}

const processTransitionMessage = message => {
    const node = nodes[message.id];
    node.removeAttribute('transition-duration');

    const moveNode = move(node);
    for (var property in message.properties) {
        moveNode.set(property, message.properties[property]);
    }

    setTimeout(() => {
        moveNode.duration(message.duration)
            .end(() => {
                if (message.destroyAfter) node.remove();
            });
    }, 20);
};

const processKeyframeMessage = message => {
    var frameProperties = Object.keys(message.properties).map(p => `${p}:${message.properties[p]}`).join();
    const frame = `${message.keyframepercent} {${frameProperties}}`;
    for (var i = 0; i < keyframesStylesheet.sheet.cssRules.length; i++) {
        if (keyframesStylesheet.sheet.cssRules[i].name === message.keyframename) {
            keyframesStylesheet.sheet.cssRules[i].appendRule(frame);
            return;
        }
    }

    keyframesStylesheet.sheet.insertRule(`@keyframes ${message.keyframename}{${frame}}`, keyframesStylesheet.length);
};

const processMessage = message => {
    if (message.message === 'SetProperty') {
        processSetPropertyMessage(message);
    } else if (message.message === 'SetTransition') {
        processTransitionMessage(message);
    } else if (message.message === 'AddKeyframe') {
        processKeyframeMessage(message);
    } else if (message.message === 'Create') {
        processCreateMessage(message);
    } else if (message.message === 'World') {
        nodes[message.id] = document.getElementById("world");
    }
};

const processMessages = messages => messages.forEach(message => processMessage(message));

setInterval(() => {
    while (isPageLoaded && messageQueue.length > messageQueueIndex) {
        processMessages(messageQueue[messageQueueIndex++]);
    }
}, 20);

connection.on("AutomationMessage", message => messageQueue.push(message));

connection.start().then(() => sendCaps()).catch(err => console.error(err.toString()));
