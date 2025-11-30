// Sticky header functionality for scoreboard
window.scoreboardStickyHeader = {
    init: function() {
        const tableHeader = document.querySelector('.table-header');
        const stickyHeader = document.querySelector('.sticky-player-header');
        const container = document.querySelector('.scoreboard-container');
        
        if (!tableHeader || !stickyHeader || !container) {
            return;
        }

        let tableHeaderOffset = null;
        
        function updateStickyHeader() {
            if (!tableHeaderOffset) {
                const rect = tableHeader.getBoundingClientRect();
                tableHeaderOffset = window.scrollY + rect.top;
            }
            
            const scrollTop = window.scrollY;
            
            // Show sticky header when original header scrolls out of view
            if (scrollTop > tableHeaderOffset) {
                stickyHeader.style.display = 'block';
                
                // Sync horizontal scroll between sticky header and table
                const containerScrollLeft = container.scrollLeft;
                stickyHeader.querySelector('.sticky-header-inner').scrollLeft = containerScrollLeft;
            } else {
                stickyHeader.style.display = 'none';
            }
        }
        
        // Sync horizontal scroll from table to sticky header
        container.addEventListener('scroll', function() {
            if (stickyHeader.style.display === 'block') {
                stickyHeader.querySelector('.sticky-header-inner').scrollLeft = container.scrollLeft;
            }
        });
        
        // Sync horizontal scroll from sticky header to table
        stickyHeader.querySelector('.sticky-header-inner').addEventListener('scroll', function() {
            container.scrollLeft = this.scrollLeft;
        });
        
        // Update on scroll
        window.addEventListener('scroll', updateStickyHeader);
        
        // Initial check
        updateStickyHeader();
        
        // Recalculate offset on resize
        window.addEventListener('resize', function() {
            tableHeaderOffset = null;
            updateStickyHeader();
        });
    },
    
    dispose: function() {
        window.removeEventListener('scroll', updateStickyHeader);
    }
};
