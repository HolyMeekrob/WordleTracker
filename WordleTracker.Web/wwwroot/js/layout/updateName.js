(function () {
	const getContainer = () => document.getElementById('user-name').parentElement;
	const getViewElement = () => document.getElementById('view-name');
	const getCopyElement = () => document.getElementById('copy-id');
	const getCopyMessageElement = () => document.getElementById('copy-message');
	const getUserId = () => document.getElementById('UserId').value;
	const getForm = () => document.getElementById('edit-name');
	const getInput = () => document.getElementById('UserName');
	const getUndo = () => document.getElementById('undo-edit-name');
	const getErrorElement = () => document.getElementById('update-name-error');

	const copyMessageTimerSeconds = 3;

	const hide = (elem) => elem.classList.add('d-none');
	const show = (elem) => elem.classList.remove('d-none');

	let savedName;

	const save = async () => {
		const shouldSave = (form) => {
			const nameHasChanged = getInput().value !== savedName;
			isValid = $(form).validate();

			return nameHasChanged && isValid;
		};

		const form = getForm();

		if (!shouldSave(form)) {
			return Promise.resolve(false);
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
				return Promise.reject(response);
			}).then(html => {
				getContainer().innerHTML = html.trim();
				handleLoad();
			}).catch(response => {
				response.text().then(msg => getErrorElement().innerText = msg);
				getInput().value = savedName;
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
		await save();
		hide(getForm());
		show(getViewElement().parentElement);
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

	const handleLoad = () => {
		addBeginEditListener();
		addCancelEditListener();
		addUndoEventListener();
		addCopyIdListener();

		savedName = getInput().value;
	};

	window.addEventListener('load', () => {
		handleLoad();
	});
})();
