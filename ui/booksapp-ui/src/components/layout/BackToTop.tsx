import { useEffect, useState } from 'react';

const BackToTop = () => {
    const [isVisible, setIsVisible] = useState(false);

    useEffect(() => {
        const handleScroll = () => {
            if (window.scrollY > 200) {
                setIsVisible(true);
            } else {
                setIsVisible(false);
            }
        };

        window.addEventListener('scroll', handleScroll);
        return () => window.removeEventListener('scroll', handleScroll);
    }, []);

    const scrollToTop = () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    return (
        <button 
            id="back-top" 
            className={`back-to-top ${isVisible ? 'active-progress' : ''}`} 
            onClick={scrollToTop}
            style={{ display: isVisible ? 'block' : 'none' }} 
        >
            <i className="fa-solid fa-chevron-up"></i>
        </button>
    );
};

export default BackToTop;