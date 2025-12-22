import { type ReactNode, useState } from 'react';
import Header from './Header';
import Footer from './Footer';
import BackToTop from './BackToTop';
import Preloader from './Preloader';
import Offcanvas from './Offcanvas';
import CursorFollower from './CursorFollower';
import AuthModals from '../auth/AuthModals';

interface LayoutProps {
    children: ReactNode;
}

const Layout = ({ children }: LayoutProps) => {
    // State to manage Offcanvas visibility
    const [isOffcanvasOpen, setIsOffcanvasOpen] = useState(false);

    const toggleOffcanvas = () => {
        setIsOffcanvasOpen(!isOffcanvasOpen);
    };

    const closeOffcanvas = () => {
        setIsOffcanvasOpen(false);
    };

    return (
        <>
            {/* 1. Cursor Follower */}
            <CursorFollower />

            {/* 2. Preloader */}
            <Preloader />

            {/* 3. Offcanvas Sidebar */}
            <Offcanvas isOpen={isOffcanvasOpen} onClose={closeOffcanvas} />

            {/* 4. Header (Pass the toggle function) */}
            <Header onOpenOffcanvas={toggleOffcanvas} />
            
            <main>
                {children}
            </main>

            {/* 5. Footer & Tools */}
            <Footer />
            <BackToTop />
            
            <AuthModals />
        </>
    );
};

export default Layout;