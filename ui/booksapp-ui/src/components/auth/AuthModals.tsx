import { useState, type FormEvent } from 'react';
import { useAuth } from '../../context/AuthContext';

const AuthModals = () => {
    const { login, register, error } = useAuth();
    
    // Login State
    const [loginEmail, setLoginEmail] = useState('');
    const [loginPass, setLoginPass] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    // Register State
    const [regDisplayName, setRegDisplayName] = useState('');
    const [regEmail, setRegEmail] = useState('');
    const [regPass, setRegPass] = useState('');

    const handleLogin = async (e: FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        try {
            await login(loginEmail, loginPass);
            // Hide Modal (using Bootstrap native close button click)
            document.getElementById('close-login-btn')?.click();
        } catch (err) {
            console.error("Login failed", err);
        } finally {
            setIsLoading(false);
        }
    };

    const handleRegister = async (e: FormEvent) => {
        e.preventDefault();
        setIsLoading(true);
        try {
            // Call the new register signature
            await register(regEmail, regPass, regDisplayName);
            document.getElementById('close-reg-btn')?.click();
        } catch (err) {
            console.error("Reg failed", err);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <>
            {/* LOGIN MODAL */}
            <div className="modal fade" id="loginModal" tabIndex={-1} aria-hidden="true">
                <div className="modal-dialog">
                    <div className="modal-content">
                        <div className="modal-body">
                            <div className="close-btn">
                                <button type="button" id="close-login-btn" className="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div className="identityBox">
                                <div className="form-wrapper">
                                    <h1>Welcome Back!</h1>
                                    <form onSubmit={handleLogin}>
                                        <input className="inputField" type="email" placeholder="Email Address" 
                                               value={loginEmail} onChange={e => setLoginEmail(e.target.value)} required />
                                        <input className="inputField" type="password" placeholder="Enter Password" 
                                               value={loginPass} onChange={e => setLoginPass(e.target.value)} required />
                                        
                                        {error && <p className="text-danger mt-2">{error}</p>}
                                        
                                        <div className="loginBtn">
                                            <button type="submit" className="theme-btn rounded-0 w-100" disabled={isLoading}>
                                                {isLoading ? 'Logging in...' : 'Log in'}
                                            </button>
                                        </div>
                                    </form>
                                    <div className="mt-3 text-center">
                                        <p>Don't have an account? <a href="#" data-bs-toggle="modal" data-bs-target="#registrationModal">Register here</a></p>
                                    </div>
                                </div>
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

            {/* REGISTRATION MODAL */}
            <div className="modal fade" id="registrationModal" tabIndex={-1} aria-hidden="true">
                <div className="modal-dialog">
                    <div className="modal-content">
                        <div className="modal-body">
                            <div className="close-btn">
                                <button type="button" id="close-reg-btn" className="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div className="identityBox">
                                <div className="form-wrapper">
                                    <h1>Create Account</h1>
                                    <form onSubmit={handleRegister}>
                                        {/* Display Name Input */}
                                        <input className="inputField" type="text" placeholder="Display Name"
                                               value={regDisplayName} onChange={e => setRegDisplayName(e.target.value)} required />
                                        
                                        {/* Email & Password */}
                                        <input className="inputField" type="email" placeholder="Email Address" 
                                               value={regEmail} onChange={e => setRegEmail(e.target.value)} required />
                                        <input className="inputField" type="password" placeholder="Enter Password" 
                                               value={regPass} onChange={e => setRegPass(e.target.value)} required />
                                        
                                        {error && <p className="text-danger mt-2">{error}</p>}

                                        <div className="loginBtn">
                                            <button type="submit" className="theme-btn rounded-0 w-100" disabled={isLoading}>
                                                {isLoading ? 'Creating Account...' : 'Register'}
                                            </button>
                                        </div>
                                    </form>
                                </div>
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
        </>
    );
};

export default AuthModals;