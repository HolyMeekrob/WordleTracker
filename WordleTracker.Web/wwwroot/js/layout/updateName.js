(function () {
	const getContainer = () => document.getElementById('user-name').parentElement;
	const getViewElement = () => document.getElementById('view-name');
	const getCopyElement = () => document.getElementById('copy-id');
	const getCopyMessageElement = () => document.getElementById('copy-message');
	const getUserId = () => document.getElementById('user-id').value;
	const getForm = () => document.getElementById('edit-name');
	const getInput = () => document.getElementById('edit-user-name');
	const getUndo = () => document.getElementById('undo-edit-name');
	const getErrorElement = () => document.getElementById('update-name-error');
	const getModal = () => document.getElementById('user-name-modal');
	const getModalInput = () => document.getElementById('edit-user-name-modal');
	const getModalSaveElement = () => document.getElementById('save-user-name');
	const getModalForm = () => document.getElementById('edit-name-modal');
	const getModalErrorElement = () => document.getElementById('update-name-error-modal');

	const copyMessageTimerSeconds = 3;
	const saveErrorText = 'Error saving user name';

	const hide = (elem) => elem.classList.add('d-none');
	const show = (elem) => elem.classList.remove('d-none');

	let savedName;

	const save = async (form) => {
		const shouldSave = () => {
			const newName = form.querySelector('[name="UserName"]').value;
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
			.then(response => {
				if (response.ok) {
					return response.text();
				}
				return Promise.reject(saveErrorText);
			}).then(html => {
				getContainer().innerHTML = html.trim();
				handleLoad();
			});
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
			await save(getForm());
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

	const addCopyIdListener = () => {
		const copyElement = getCopyElement();
		copyElement.addEventListener('click', () => {
			navigator.clipboard.writeText(getUserId())
				.then(() => {
					const messageElement = getCopyMessageElement();
					hide(copyElement);
					show(messageElement);
					setTimeout(() => {
						hide(messageElement);
						show(copyElement);
					}, copyMessageTimerSeconds * 1000);
				});
		});
	};

	const initializeModal = () => {
		const modal = getModal();
		const input = getModalInput();
		const errorElement = getModalErrorElement();

		modal.addEventListener('show.bs.modal', () => {
			input.value = savedName;
			errorElement.innerText = '';
		});

		modal.addEventListener('shown.bs.modal', () => {
			input.select();
			input.focus();
		});

		getModalSaveElement().addEventListener('click', async () => {
			try {
				await save(getModalForm());
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
		addCopyIdListener();

		initializeModal();

		savedName = getInput().value;
	};

	window.addEventListener('load', () => {
		handleLoad();
	});
})();
