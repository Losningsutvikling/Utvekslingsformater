function postForm(path) {
    document.forms[0].action = path;
    document.forms[0].submit();
}

function toggleOptionalBox(element) {
    el = $('#' + element);
    el.toggleClass('hidden');
    checkDependentControlsActivationById(element);
}

function setHiddenByCheckbox(element, controlledElement) {
    el = $('#' + element);
    var checked = document.getElementById(element).checked;
    controlledEl = $('#' + controlledElement);
    controlledEl.removeClass('hidden');
    if (!checked) {
        controlledEl.addClass('hidden');
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
function checkDependentControlsActivation(element) {
    foundControl = controlDependencies.find((item) => element && element.hasAttribute('id') && item.activatorControl == element.id);
    if (foundControl != null) {
        //window.alert('Fant avhengighet: ControlId:' + foundControl.control + ', verdi:' + foundControl.value);
        var enabled = element.type == 'checkbox' ? (foundControl.value == (element.checked ? '1' : '0')) : foundControl.value == element.value;
        //var el = document.getElementById(foundControl.control);
        //el.disabled = !enabled;
        el = $('#' + foundControl.control);
        el.prop('disabled', !enabled);
        el.parent().removeClass('disabled_variable');
        if (!enabled) {
            el.parent().addClass('disabled_variable');
        }
    }
}

function createNewFromTemplate(divId) {
    counterEl = document.getElementById(divId + '_counter');
    count = parseInt(counterEl.value);
    templateElement = document.getElementById(divId + '_template');
    templateContents = templateElement.innerHTML.replace('_template', '__' + count + '__').replace('hidden_template', '');

    div = document.createElement('div');
    div.innerHTML = templateContents;
    buttonElement = document.getElementById(divId + '_new');
    templateElement.parentNode.insertBefore(div, buttonElement);
    count++;
    counterEl.value = count.toString();

    maxEl = document.getElementById(divId + '_maxOccurs');
    maxCount = parseInt(maxEl.value);
    if (count >= maxCount) {
        buttonElement.disabled = true;
    }

}