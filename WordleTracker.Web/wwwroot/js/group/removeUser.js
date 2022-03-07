(function () {
	const getGroupId = () => document.getElementById('Id').value;
	const getRemoveUserElements = () => document.querySelectorAll('[data-action="remove-user"]');
	const getRequestVerificationToken = () => document.querySelector('input[name="__RequestVerificationToken"]').value;

	const removeUser = async (event) => {
		const userId = event.target.dataset.id;
		const groupId = getGroupId();

		const options = {
			method: 'DELETE',
			headers: {
				RequestVerificationToken: getRequestVerificationToken(),
				'Content-Type': 'application/json'
			},
			body: JSON.stringify({
				userId
			})
		};

		fetch(`/Group/${groupId}?handler=user`, options)
			.then(response => {
				if (response.ok) {
					// Prevent post if back button is pressed
					window.history.replaceState(null, null, window.location.href);
					window.location = window.location.href;
				} else {

				}
			});
	};

	const addRemoveUserListeners = () => {
		const elements = getRemoveUserElements();

		for (const element of elements) {
			element.addEventListener('click', removeUser);
		}
	};

	const handleLoad = () => {
		addRemoveUserListeners();
	};

	window.addEventListener('load', handleLoad);
})();
