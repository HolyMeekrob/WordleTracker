(function () {
	const getViewElement = () => document.getElementById('view-group-name');
	const getForm = () => document.getElementById('edit-group-name');
	const getInput = () => document.getElementById('edit-group-name-input');
	const getUndo = () => document.getElementById('undo-edit-group-name');
	const getErrorElement = () => document.getElementById('update-group-name-error');

	const saveErrorText = 'Error saving group name';

	const hide = (elem) => elem.classList.add('d-none');
	const show = (elem) => elem.classList.remove('d-none');

	let savedName;

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
				? Promise.resolve(getInput().value)
				: Promise.reject(saveErrorText));
	};

	const addBeginEditListener = () => {
		const viewElement = getViewElement();
		viewElement.addEventListener('click', () => {
			hide(viewElement.parentElement);
			show(getForm());

			const input = getInput();
			input.select();
			input.focus();
		});
	};

	const hideEdit = async () => {
		try {
			var newName = await save(getForm());

			getViewElement().innerText = newName;
			savedName = newName;

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

	const handleLoad = () => {
		addBeginEditListener();
		addCancelEditListener();
		addUndoEventListener();

		savedName = getInput().value;
	};

	window.addEventListener('load', handleLoad);
})();
