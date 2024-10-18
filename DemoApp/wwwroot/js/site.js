function postForm(path) {
    document.forms[0].action = path;
    document.forms[0].submit();
}

function setHiddenByCheckbox(element, controlledElement) {
    var checked = document.getElementById(element).checked;
    controlledEl = document.getElementById(controlledElement);

    controlledEl.classList.remove('hidden');
    if (!checked) {
        controlledEl.classList.add('hidden');
    }
    checkDependentControlsActivationById(element);
}

var controlDependencies = [];
function registerDependentControl(controlId, activatorControlId, activatingValue) {
    controlDependencies.push({activatorControl: activatorControlId, control: controlId, value: activatingValue });
}

function setDependentControlsDefault() {
    controlDependencies.forEach((item) => checkDependentControlsActivationById(item.activatorControl));
}

function checkDependentControlsActivationById(elementId) {
    var el = document.getElementById(elementId);
    checkDependentControlsActivation(el);
}

function toggleCollapseContainer(element) {
    for (i = 0; i < element.children.length; i++) {
        child = element.children[i];
        if (child.tagName == 'DIV') {
            child.classList.toggle('hidden');
            break;
        }
    }
    for (i = 0; i < element.children.length; i++) {
        child = element.children[i];
        if (child.tagName == 'IMG') {
            if (child.src.includes('up')){
                child.src = child.src.replace('up', 'right');
            }
            else {
                child.src = child.src.replace('right', 'up');
            }

// Forandre bildesource eller vise/gjemme to bilder motsatt 
            break;
        }
    }
}

function checkDependentControlsActivation(element) {
    foundControl = controlDependencies.find((item) => element && element.hasAttribute('id') && item.activatorControl == element.id);
    if (foundControl != null) {
        //window.alert('Fant avhengighet: ControlId:' + foundControl.control + ', verdi:' + foundControl.value);
        var enabled = element.type == 'checkbox' ? (foundControl.value == (element.checked ? '1' : '0')) : foundControl.value == element.value;
        //var el = document.getElementById(foundControl.control);
        //el.disabled = !enabled;
        el = document.getElementById(foundControl.control);
        el.disabled = !enabled;
        el.parentElement.classList.remove('disabled_variable');
        if (!enabled) {
            el.parentElement.classList.add('disabled_variable');
        }
    }
}

function createNewFromTemplate(divId) {
    counterEl = document.getElementById(divId + '_counter');
    count = parseInt(counterEl.value);
    count++;
    templateElement = document.getElementById(divId + '_template');
    templateContents = templateElement.innerHTML.replaceAll(':0:template', ':' + count).replaceAll('hidden_template', '');

    div = document.createElement('div');
    div.classList.add('expanded_template');
    div.innerHTML = templateContents;
    buttonElement = document.getElementById(divId + '_new');
    templateElement.parentNode.insertBefore(div, buttonElement);
    counterEl.value = count.toString();

    maxEl = document.getElementById(divId + '_maxOccurs');
    maxCount = parseInt(maxEl.value);
    if (count >= maxCount) {
        buttonElement.disabled = true;
    }
    setDependentControlsDefault();
}

