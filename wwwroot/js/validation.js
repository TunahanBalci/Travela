function validatePasswordsMatch() {
    const password = document.getElementById('Password').value;
    const confirmPassword = document.getElementById('ConfirmPassword').value;

    if (password !== confirmPassword) {
        alert('Passwords do not match. Please try again.');
        return false;
    }

    return true;
}
