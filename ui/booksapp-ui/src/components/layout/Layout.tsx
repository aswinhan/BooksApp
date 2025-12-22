import type { ReactNode } from 'react';
import Header from './Header';
import Footer from './Footer';
import BackToTop from './BackToTop';
import Preloader from './Preloader';
import AuthModals from '../auth/AuthModals';

interface LayoutProps {
    children: ReactNode;
}

const Layout = ({ children }: LayoutProps) => {
    return (
        <>
            <div className="cursor-follower"></div>

            <Preloader />

            <Header />
            
            <main>
                {children}
            </main>

            <Footer />
            <BackToTop />
            
            {/* Include the Modals here so they are available everywhere */}
            <AuthModals />
        </>
    );
};

export default Layout;