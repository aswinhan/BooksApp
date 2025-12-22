import { useEffect, useRef } from 'react';

const CursorFollower = () => {
    const cursorRef = useRef<HTMLDivElement>(null);
    
    // We use refs instead of state to avoid re-rendering 60 times a second
    const position = useRef({ mouseX: 0, mouseY: 0, followerX: 0, followerY: 0 });

    useEffect(() => {
        // 1. Update mouse coordinates on move
        const handleMouseMove = (e: MouseEvent) => {
            position.current.mouseX = e.clientX;
            position.current.mouseY = e.clientY;
        };

        // 2. The Animation Loop (Replaces TweenMax)
        const animate = () => {
            const { mouseX, mouseY, followerX, followerY } = position.current;

            // The "Lag" Logic: Move 1/9th of the way to the mouse
            const distX = mouseX - followerX;
            const distY = mouseY - followerY;
            
            position.current.followerX += distX / 9;
            position.current.followerY += distY / 9;

            // Apply visual update
            if (cursorRef.current) {
                // Subtract 15 (half width) to center it. The original code used 12.
                // We use translate3d for GPU acceleration (smoother than top/left)
                const x = position.current.followerX - 15; 
                const y = position.current.followerY - 15;
                cursorRef.current.style.transform = `translate3d(${x}px, ${y}px, 0)`;
            }

            // Keep looping
            requestAnimationFrame(animate);
        };

        window.addEventListener('mousemove', handleMouseMove);
        const animationId = requestAnimationFrame(animate);

        return () => {
            window.removeEventListener('mousemove', handleMouseMove);
            cancelAnimationFrame(animationId);
        };
    }, []);

    return <div ref={cursorRef} className="cursor-follower"></div>;
};

export default CursorFollower;