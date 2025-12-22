import { useEffect, useState } from 'react';

const BackToTop = () => {
    const [isVisible, setIsVisible] = useState(false);

    useEffect(() => {
        const toggleVisibility = () => {
            if (window.scrollY > 300) {
                setIsVisible(true);
            } else {
                setIsVisible(false);
            }
        };

        window.addEventListener('scroll', toggleVisibility);
        return () => window.removeEventListener('scroll', toggleVisibility);
    }, []);

    const scrollToTop = () => {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    };

    return (
        <button 
            id="back-top" 
            className={`back-to-top ${isVisible ? 'd-block' : 'd-none'}`} 
            onClick={scrollToTop}
            style={{ display: isVisible ? 'block' : 'none' }} // Inline style fallback
        >
            <i className="fa-solid fa-chevron-up"></i>
        </button>
    );
};

export default BackToTop;