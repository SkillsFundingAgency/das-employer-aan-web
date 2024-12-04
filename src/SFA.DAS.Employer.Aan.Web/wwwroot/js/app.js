function AutoComplete(selectField) {
    this.selectElement = selectField
}

AutoComplete.prototype.init = function () {
    this.autoComplete()
}

function nodeListForEach(nodes, callback) {
    if (window.NodeList.prototype.forEach) {
        return nodes.forEach(callback)
    }
    for (let i = 0; i < nodes.length; i++) {
        callback.call(window, nodes[i], i, nodes);
    }
}

let autoCompletes = document.querySelectorAll('[data-module="autoComplete"]')

nodeListForEach(autoCompletes, function (autoComplete) {
    new AutoComplete(autoComplete).init()
})

// Select Submit
function SelectSubmit(selectField) {
    this.selectField = selectField;
    this.form = selectField.closest("form");
}

SelectSubmit.prototype.init = function () {
    if (!this.selectField) {
        return;
    }
    this.selectField.addEventListener("change", () => {
        this.form.submit();
    });
};

const selectSubmits = document.querySelectorAll('[data-module="selectSubmit"]');
nodeListForEach(selectSubmits, function (selectSubmit) {
    console.log(selectSubmit);
    new SelectSubmit(selectSubmit).init();
});


//Location search autocomplete
const locationInputs = document.querySelectorAll(".location-search-autocomplete");
const apiUrl = "/locations/search";

if (locationInputs.length > 0) {
    for (let i = 0; i < locationInputs.length; i++) {
        const input = locationInputs[i];
        const container = document.createElement("div");
        const withinSelect = document.getElementById("within");

        container.className = "das-autocomplete-wrap";
        container.dataset.trackUserSelected = input.dataset.trackUserSelected;
        input.parentNode.replaceChild(container, input);

        const getSuggestions = async (query, updateResults) => {
            const results = [];
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    let results = JSON.parse(xhr.responseText);
                    results = results.map(function (r) {
                        return r.name;
                    });
                    updateResults(results);
                }
            };
            xhr.open("GET", `${apiUrl}?query=${query}`, true);
            xhr.send();
        };

        accessibleAutocomplete({
            element: container,
            id: input.id,
            name: input.name,
            defaultValue: input.value,
            displayMenu: "overlay",
            showNoOptionsFound: false,
            minLength: 2,
            source: getSuggestions,
            placeholder: "",
            confirmOnBlur: false,
            autoselect: true,
            onConfirm: () => {
                const trackSelection = input.dataset.trackUserSelected;
                if (trackSelection) {
                    const hiddenField = document.getElementById(trackSelection);
                    if (hiddenField) {
                        hiddenField.value = "true";
                    }
                }
                if (withinSelect) {
                    if (withinSelect.value === "all") {
                        withinSelect.value = "10";
                    }
                }
            },
        });
    }

    const autocompleteInputs = document.querySelectorAll(".autocomplete__input");
    if (autocompleteInputs.length > 0) {
        for (let i = 0; i < autocompleteInputs.length; i++) {
            const autocompleteInput = autocompleteInputs[i];
            autocompleteInput.setAttribute("autocomplete", "new-password");
        }
    }
}
document.addEventListener("DOMContentLoaded", function () {
    const pageIdentifier = document.getElementById('event-types-container');
    if (!pageIdentifier) return;

    const allCheckbox = document.getElementById("all-checkbox");
    const otherCheckboxes = document.querySelectorAll(".other-checkbox");

    allCheckbox.addEventListener("change", function () {
        if (allCheckbox.checked) {
            otherCheckboxes.forEach(checkbox => {
                checkbox.checked = false;
            });
        }
    });

    otherCheckboxes.forEach(checkbox => {
        checkbox.addEventListener("change", function () {
            if (checkbox.checked) {
                allCheckbox.checked = false;
            }
        });
    });
});
