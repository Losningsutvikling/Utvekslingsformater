function postForm(path, actionParam) {
    document.forms[0].action = path;
    if (actionParam) {
        document.getElementById('actionParam').value = actionParam;
    }
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
function registerDependentControl(controlId, activatorControlId, activatingValue, isInverted) {
    controlDependencies.push({activatorControl: activatorControlId, control: controlId, value: activatingValue, inverted: isInverted });
}

function setDependentControlsDefault() {
    controlDependencies.forEach((item) => checkDependentControlsActivationById(item.activatorControl));
}

function checkDependentControlsActivationById(elementId) {
    var el = document.getElementById(elementId);
    if (el == null) {
        elements = document.querySelectorAll('[id ^= "' + elementId + ':"]');
        if (elements.length > 0)
            elements.forEach((item) => checkDependentControlsActivation(item));
    }
    else
        checkDependentControlsActivation(el);
}

function toggleCollapseContainer(elementId) {
    element = document.getElementById(elementId);
    for (i = 0; i < element.children.length; i++) {
        child = element.children[i];
        if (child != element && child.tagName == 'DIV' && !(child.id ?? "").startsWith("toggleControl_")) {
            child.classList.toggle('hidden');
        }
    }
    img = (element.children[0].tagName == "IMG") ? element.children[0] : ((element.children[0].children[0].tagName == "IMG") ? element.children[0].children[0] : null); 
    if (img) {
        if (img.src.includes('up')) {
            img.src = img.src.replace('up', 'right');
        }
        else {
            img.src = img.src.replace('right', 'up');
        }
    }
}

function checkDependentControlsActivation(element) {
    var isIteratedControlName = false;
    foundControls = controlDependencies.filter((item) => element && element.hasAttribute('id') && item.activatorControl == element.id);
    if (foundControls == null || foundControls.length == 0) {
        /*Problem: Checkbox i 'Iterator.cshtml' ligger med id="XXXXX:<enumverdi>", f.eks. "HenvisningHjelpetiltak.PlanEtterOnsketTiltak.Plan:9"*/
        foundControls = controlDependencies.filter((item) => element && element.hasAttribute('id') && element.id.startsWith(item.activatorControl + ":"));
        isIteratedControlName = true;
    }
    if (foundControls != null && foundControls.length > 0)
    {
        for (i = 0; i < foundControls.length; i++) {
            foundControl = foundControls[i];
            foundControlValue = foundControl.value ?? "";
            foundValue = foundControlValue.split(',');
            var doProcess = true;
            var enabled = false;
            if (element.type == 'checkbox') {
                if (isIteratedControlName) {
                    doProcess = foundValue.includes(element.value);
                    if (doProcess)
                        enabled = element.checked;
                }
                else {
                    enabled = foundControl.value == (element.checked ? '1' : '0');
                }
            }
            else {
                enabled = foundValue.includes(element.value);
            }
            if (foundControl.inverted == 1) {
                enabled = !enabled;
            }
            if (doProcess) {
                el = document.getElementById(foundControl.control);
                el.disabled = !enabled;
                el.parentElement.classList.remove('disabled_variable');
                if (!enabled) {
                    el.parentElement.classList.add('disabled_variable');
                }
            }
        }
    }
}

function createNewFromTemplate(divId) {
    counterEl = document.getElementById(divId + '_counter');
    count = parseInt(counterEl.value);
    count++;
    templateElement = document.getElementById(divId + '_template');
    templateContents = templateElement.innerHTML.replaceAll(':0.template', ':' + count).replaceAll('hidden_template', '');

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

function chooseChoiceElement(id, selectElementClass) {
    var choiceElement = document.getElementById(id);
    var parentElement = choiceElement.parentElement;
    for (i = 0; i < parentElement.children.length; i++)
    {
        ch = parentElement.children[i];
        if (ch.classList.contains(selectElementClass)) {
            ch.classList.toggle('hidden');
        }
        if (ch.children.length > 0) {
            for (j = 0; j < ch.children.length; j++) {
                grandCh = ch.children[j];
                if (grandCh.classList.contains('obligatorisk_markor')) {
                    grandCh.classList.toggle('disabled');
                }
            }
        }
    }
}

function menuActionMeldingListe(e, url, actionParameters) {
    var menuDiv = e.currentTarget.parentElement.parentElement;
    var row = menuDiv.parentElement.parentElement;
    var idInput = row.children[0].children[0]; // hidden input
    document.getElementById('XML_FIL').value = idInput.value;
    form = idInput.form;
    form.action = url;
    if (actionParameters) {
        var arr = actionParameters.split(';');
        arr.forEach((param) => {
            paramArr = param.split('=');
            var el = document.getElementById(paramArr[0]);
            var value = (paramArr.length > 1) ? paramArr[1] : "";
            el.value = value;
        });
    }
    form.submit();
}

