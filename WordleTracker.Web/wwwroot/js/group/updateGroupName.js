(function () {
	const getViewElement = () => document.getElementById('view-group-name');
	const getViewElementNarrow = () => document.getElementById('view-group-name-narrow');

	const getForm = () => document.getElementById('edit-group-name');
	const getInput = () => document.getElementById('edit-group-name-input');
	const getUndo = () => document.getElementById('undo-edit-group-name');
	const getErrorElement = () => document.getElementById('update-group-name-error');

	const getModal = () => document.getElementById('group-name-modal');
	const getModalInput = () => document.getElementById('edit-group-name-input-modal');
	const getModalSaveElement = () => document.getElementById('save-group-name');
	const getModalForm = () => document.getElementById('edit-group-name-modal');
	const getModalErrorElement = () => document.getElementById('update-group-name-error-modal');

	const saveErrorText = 'Error saving group name';

	const hide = (elem) => elem.classList.add('d-none');
	const show = (elem) => elem.classList.remove('d-none');

	let savedName;

	const updateGroupName = (name) => {
		getViewElement().innerText = name;
		getViewElementNarrow().innerText = name;
		savedName = name;
	};

	const save = async (form) => {
		const shouldSave = () => {
			const newName = form.querySelector('[name="Name"]').value;
			const nameHasChanged = newName !== savedName;

			return nameHasChanged;
		};

		if (!shouldSave(form)) {
			return Promise.reject();
		}

		const options = {
			method: form.method,
			body: new FormData(form)
		};

		return fetch(form.action, options)
			.then(response => response.ok
				? response.text()
				: Promise.reject(saveErrorText));
	};

	const addBeginEditListener = () => {
		const viewElement = getViewElement();
		viewElement.addEventListener('click', () => {
			const input = getInput();
			input.value = savedName;

			hide(viewElement.parentElement);
			show(getForm());

			input.select();
			input.focus();
		});
	};

	const hideEdit = async () => {
		try {
			var newName = await save(getForm());

			updateGroupName(newName);

			hide(getForm());
			show(getViewElement().parentElement);
		} catch (err) {
			getInput().value = savedName;
			if (err) {
				getErrorElement().innerText = err;
			}
			hide(getForm());
			show(getViewElement().parentElement);
		}
	};

	const addCancelEditListener = () => {
		getInput().addEventListener('blur', (evt) => {
			if (!evt.relatedTarget || evt.relatedTarget.id !== getUndo().id) {
				hideEdit();
			}
		});

		getUndo().addEventListener('blur', (evt) => {
			if (!evt.relatedTarget || evt.relatedTarget.id !== getInput().id) {
				hideEdit();
			}
		});
	};

	const addUndoEventListener = () => {
		getUndo().addEventListener('click', () => {
			getInput().value = savedName;
			hideEdit();
		});
	};

	const initializeModal = () => {
		const modal = getModal();
		const input = getModalInput();
		const form = getModalForm();
		const saveButton = getModalSaveElement();
		const errorElement = getModalErrorElement();

		modal.addEventListener('show.bs.modal', () => {
			input.value = savedName;
			errorElement.innerText = '';
		});

		modal.addEventListener('shown.bs.modal', () => {
			input.select();
			input.focus();
		});

		saveButton.addEventListener('click', async () => {
			try {
				var newName = await save(form);

				updateGroupName(newName);

				bootstrap.Modal.getInstance(modal).hide();
			} catch (err) {
				if (err) {
					errorElement.innerText = err;
				}
			}
		});
	};

	const handleLoad = () => {
		addBeginEditListener();
		addCancelEditListener();
		addUndoEventListener();

		initializeModal();

		savedName = getInput().value;
	};

	window.addEventListener('load', handleLoad);
})();
