import { useEffect, useState } from 'react';

const Preloader = () => {
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        // Simulating the jQuery window.load event
        const timer = setTimeout(() => {
            setLoading(false);
        }, 1000); // Adjust time or bind to actual window load

        return () => clearTimeout(timer);
    }, []);

    if (!loading) return null;

    return (
        <div id="preloader" className="preloader">
            <div className="animation-preloader">
                <div className="spinner"></div>
                <div className="txt-loading">
                    {['B', 'O', 'O', 'K', 'L', 'E'].map((char, index) => (
                        <span key={index} data-text-preloader={char} className="letters-loading">
                            {char}
                        </span>
                    ))}
                </div>
                <p className="text-center">Loading</p>
            </div>
            <div className="loader">
                <div className="row">
                    <div className="col-3 loader-section section-left"><div className="bg"></div></div>
                    <div className="col-3 loader-section section-left"><div className="bg"></div></div>
                    <div className="col-3 loader-section section-right"><div className="bg"></div></div>
                    <div className="col-3 loader-section section-right"><div className="bg"></div></div>
                </div>
            </div>
        </div>
    );
};

export default Preloader;