const AuthModals = () => {
    return (
        <>
            {/* Login Modal */}
            <div className="modal fade" id="loginModal" tabIndex={-1} aria-labelledby="loginModalLabel" aria-hidden="true">
                <div className="modal-dialog">
                    <div className="modal-content">
                        <div className="modal-body">
                            <div className="close-btn">
                                <button type="button" className="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div className="identityBox">
                                <div className="form-wrapper">
                                    <h1 id="loginModalLabel">welcome back!</h1>
                                    <input className="inputField" type="email" name="email" placeholder="Email Address" />
                                    <input className="inputField" type="password" name="password" placeholder="Enter Password" />
                                    {/* ... rest of login form ... */}
                                    <div className="loginBtn">
                                        <button className="theme-btn rounded-0 w-100">Log in</button>
                                    </div>
                                </div>
                                {/* Banner Side */}
                                <div className="banner">
                                    <div className="loginBg">
                                        <img src="/assets/img/signUpbg.jpg" alt="signUpBg" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Registration Modal - Structure similar to Login */}
             <div className="modal fade" id="registrationModal" tabIndex={-1} aria-labelledby="registrationModalLabel" aria-hidden="true">
                {/* ... Paste Registration Modal HTML here, converting class to className ... */}
             </div>
        </>
    );
};

export default AuthModals;