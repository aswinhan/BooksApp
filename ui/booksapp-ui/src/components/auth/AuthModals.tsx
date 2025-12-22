const AuthModals = () => {
  return (
    <>
      {/* Login Modal */}
      <div
        className="modal fade"
        id="loginModal"
        tabIndex={-1}
        aria-labelledby="loginModalLabel"
        aria-hidden="true"
      >
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-body">
              <div className="close-btn">
                <button
                  type="button"
                  className="btn-close"
                  data-bs-dismiss="modal"
                  aria-label="Close"
                ></button>
              </div>
              <div className="identityBox">
                <div className="form-wrapper">
                  <h1 id="loginModalLabel">welcome back!</h1>
                  <input
                    className="inputField"
                    type="email"
                    name="email"
                    placeholder="Email Address"
                  />
                  <input
                    className="inputField"
                    type="password"
                    name="password"
                    placeholder="Enter Password"
                  />
                  <div className="input-check remember-me">
                    <div className="checkbox-wrapper">
                      <input
                        type="checkbox"
                        className="form-check-input"
                        name="save-for-next"
                        id="saveForNext"
                      />
                      <label htmlFor="saveForNext">Remember me</label>
                    </div>
                    <div className="text">
                      {" "}
                      <a href="/">Forgot Your password?</a>{" "}
                    </div>
                  </div>
                  <div className="loginBtn">
                    <button className="theme-btn rounded-0 w-100">
                      Log in
                    </button>
                  </div>
                  <div className="orting-badge">Or</div>
                  <div>
                    <a
                      className="another-option"
                      href="https://www.google.com/"
                    >
                      <img src="assets/img/google.png" alt="google" />
                      Continue With Google
                    </a>
                  </div>
                  <div>
                    <a
                      className="another-option another-option-two"
                      href="https://www.facebook.com/"
                    >
                      <img src="assets/img/facebook.png" alt="google" />
                      Continue With Facebook
                    </a>
                  </div>

                  <div className="form-check-3 d-flex align-items-center from-customradio-2 mt-3">
                    <input
                      className="form-check-input"
                      type="radio"
                      name="flexRadioDefault"
                    />
                    <label className="form-check-label">
                      I Accept Your Terms & Conditions
                    </label>
                  </div>
                </div>

                <div className="banner">
                  <button
                    type="button"
                    className="rounded-0 login-btn"
                    data-bs-toggle="modal"
                    data-bs-target="#loginModal"
                  >
                    Log in
                  </button>
                  <button
                    type="button"
                    className="theme-btn rounded-0 register-btn"
                    data-bs-toggle="modal"
                    data-bs-target="#registrationModal"
                  >
                    Create Account
                  </button>
                  <div className="loginBg">
                    <img src="assets/img/signUpbg.jpg" alt="signUpBg" />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Registration Modal */}
      <div
        className="modal fade"
        id="registrationModal"
        tabIndex={-1}
        aria-labelledby="registrationModalLabel"
        aria-hidden="true"
      >
        <div className="modal-dialog">
            <div className="modal-content">
                <div className="modal-body">
                    <div className="close-btn">
                        <button type="button" className="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div className="identityBox">
                        <div className="form-wrapper">
                            <h1 id="registrationModalLabel">Create account!</h1>
                            <input className="inputField" type="text" name="name" id="name" placeholder="User Name"/>
                            <input className="inputField" type="email" name="email" placeholder="Email Address"/>
                            <input className="inputField" type="password" name="password" placeholder="Enter Password"/>
                            <input className="inputField" type="password" name="password"
                                placeholder="Enter Confirm Password"/>
                            <div className="input-check remember-me">
                                <div className="checkbox-wrapper">
                                    <input type="checkbox" className="form-check-input" name="save-for-next" id="rememberMe"/>
                                    <label htmlFor="rememberMe">Remember me</label>
                                </div>
                                <div className="text"> <a href="index-2.html">Forgot Your password?</a> </div>
                            </div>
                            <div className="loginBtn">
                                <a href="index-2.html" className="theme-btn rounded-0"> Log in </a>
                            </div>
                            <div className="orting-badge">
                                Or
                            </div>
                            <div>
                                <a className="another-option" href="https://www.google.com/">
                                    <img src="assets/img/google.png" alt="google"/>
                                    Continue With Google
                                </a>
                            </div>
                            <div>
                                <a className="another-option another-option-two" href="https://www.facebook.com/">
                                    <img src="assets/img/facebook.png" alt="google"/>
                                    Continue With Facebook
                                </a>
                            </div>
                            <div className="form-check-3 d-flex align-items-center from-customradio-2 mt-3">
                                <input className="form-check-input" type="radio" name="flexRadioDefault"/>
                                <label className="form-check-label">
                                    I Accept Your Terms & Conditions
                                </label>
                            </div>
                        </div>

                        <div className="banner">
                            <button type="button" className="rounded-0 login-btn" data-bs-toggle="modal"
                                data-bs-target="#loginModal">Log in</button>
                            <button type="button" className="theme-btn rounded-0 register-btn" data-bs-toggle="modal"
                                data-bs-target="#registrationModal">Create
                                Account</button>
                            <div className="signUpBg">
                                <img src="assets/img/registrationbg.jpg" alt="signUpBg"/>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
      </div>
    </>
  );
};

export default AuthModals;
