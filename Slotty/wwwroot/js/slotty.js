/**
 * HighlightOverlay Class
 * Advanced highlighting system with CSS variable integration
 */
class HighlightOverlay {
    constructor(options = {}) {
      // Get CSS variables from document
      const computedStyle = getComputedStyle(document.documentElement);
      
      this.options = {
        borderRadius: parseInt(computedStyle.getPropertyValue('--slotty-overlay-border-radius')) || options.borderRadius || 8,
        transitionDuration: parseInt(computedStyle.getPropertyValue('--slotty-overlay-transition-duration')) || options.transitionDuration || 400,
        overlayColor: computedStyle.getPropertyValue('--slotty-overlay-background-color').trim() || options.overlayColor || 'rgba(59, 130, 246, 0.2)',
        sheenColor: computedStyle.getPropertyValue('--slotty-overlay-sheen-color').trim() || options.sheenColor || 'rgba(255, 255, 255, 0.3)',
        ...options
      };
      
      this.overlays = new Map();
      this.isVisible = false;
      this.animationId = null;
    }
    
    createOverlay(element, name) {
      const overlay = document.createElement('div');
      overlay.className = 'slotty-overlay';
      
      // Add name pill if name is provided
      if (name) {
        const namePill = document.createElement('div');
        namePill.className = 'slotty-overlay-name-pill';
        namePill.textContent = name;
        overlay.appendChild(namePill);
      }
      
      document.body.appendChild(overlay);
      this.updateOverlayPosition(overlay, element);
      
      return overlay;
    }
    
    updateOverlayPosition(overlay, element) {
      let rect = element.getBoundingClientRect();
      const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
      const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;
      
      // Handle display: contents elements that don't generate layout boxes
      const elementStyle = window.getComputedStyle(element);
      if (elementStyle.display === 'contents') {
        rect = this.getContentsElementRect(element);
      }
      
      // Handle edge cases for dimensions
      const minWidth = 20; // Minimum visible width
      const minHeight = 20; // Minimum visible height
      const maxWidth = window.innerWidth * 0.9; // Maximum reasonable width
      const maxHeight = window.innerHeight * 0.9; // Maximum reasonable height
      
      let { width, height } = rect;
      let { top, left } = rect;
      
      // Detect and handle problematic dimensions
      const isFlexChild = this.isFlexOrGridChild(element);
      const isEmpty = this.isEmptyElement(element);
      
      // Handle zero or very small dimensions
      if (width < minWidth || height < minHeight) {
        console.warn(`Element has very small dimensions (${width}x${height}). Adjusting overlay size.`);
        
        // For empty elements, use minimum dimensions
        if (isEmpty) {
          width = Math.max(width, minWidth);
          height = Math.max(height, minHeight);
        } else {
          // For elements with content, try to get the content dimensions
          const contentRect = this.getContentDimensions(element);
          width = Math.max(width, contentRect.width, minWidth);
          height = Math.max(height, contentRect.height, minHeight);
        }
      }
      
      // Handle extremely large dimensions (possibly layout issues)
      if (width > maxWidth || height > maxHeight) {
        console.warn(`Element has very large dimensions (${width}x${height}). This might indicate a layout issue.`);
        // Don't automatically adjust - let user see the issue, but cap for performance
        width = Math.min(width, maxWidth);
        height = Math.min(height, maxHeight);
      }
      
      // Handle flex/grid children that might be positioned unusually
      if (isFlexChild) {
        const parentRect = element.parentElement.getBoundingClientRect();
        
        // Check if element is overflowing its parent
        if (left + width > parentRect.right || top + height > parentRect.bottom) {
          console.info('Element appears to be overflowing its flex/grid parent');
        }
        
        // Check for flex items that might be shrunk to zero
        if (elementStyle.flexShrink !== '0' && (width < minWidth || height < minHeight)) {
          console.info('Flex item appears to be shrunk. Original dimensions might be larger.');
        }
      }
      
      // Adjust position to account for border width
      // Get border width from computed styles since it's now part of --slotty-overlay-border
      const borderWidth = parseInt(getComputedStyle(document.documentElement).getPropertyValue('--slotty-overlay-border')) || 2;
      const adjustedTop = top + scrollTop - borderWidth;
      const adjustedLeft = left + scrollLeft - borderWidth;
      const adjustedWidth = width + (borderWidth * 2);
      const adjustedHeight = height + (borderWidth * 2);
      
      // Apply the calculated dimensions
      overlay.style.top = `${adjustedTop}px`;
      overlay.style.left = `${adjustedLeft}px`;
      overlay.style.width = `${adjustedWidth}px`;
      overlay.style.height = `${adjustedHeight}px`;
      
      // Add data attributes for debugging
      overlay.dataset.originalWidth = width;
      overlay.dataset.originalHeight = height;
      overlay.dataset.isFlexChild = isFlexChild;
      overlay.dataset.isEmpty = isEmpty;
    }
    
    getContentsElementRect(element) {
      // For display: contents elements, calculate bounding rect from children
      const children = Array.from(element.children).filter(child => {
        const style = window.getComputedStyle(child);
        return style.display !== 'none';
      });
      
      if (children.length === 0) {
        // No visible children, use a minimal rect at the parent's position
        const parentRect = element.parentElement?.getBoundingClientRect() || { top: 0, left: 0, width: 0, height: 0 };
        return {
          top: parentRect.top,
          left: parentRect.left,
          width: 100, // Minimum visible width
          height: 30  // Minimum visible height
        };
      }
      
      // Calculate the union of all children's bounding rects
      let minTop = Infinity, minLeft = Infinity, maxBottom = -Infinity, maxRight = -Infinity;
      
      children.forEach(child => {
        const childRect = child.getBoundingClientRect();
        minTop = Math.min(minTop, childRect.top);
        minLeft = Math.min(minLeft, childRect.left);
        maxBottom = Math.max(maxBottom, childRect.bottom);
        maxRight = Math.max(maxRight, childRect.right);
      });
      
      return {
        top: minTop,
        left: minLeft,
        width: maxRight - minLeft,
        height: maxBottom - minTop
      };
    }

    isFlexOrGridChild(element) {
      if (!element.parentElement) return false;
      
      const parentStyle = window.getComputedStyle(element.parentElement);
      return parentStyle.display === 'flex' || 
             parentStyle.display === 'inline-flex' ||
             parentStyle.display === 'grid' ||
             parentStyle.display === 'inline-grid';
    }
    
    isEmptyElement(element) {
      // Check if element has no meaningful content
      const hasTextContent = element.textContent.trim().length > 0;
      const hasChildElements = element.children.length > 0;
      const hasBackgroundImage = window.getComputedStyle(element).backgroundImage !== 'none';
      
      return !hasTextContent && !hasChildElements && !hasBackgroundImage;
    }
    
    getContentDimensions(element) {
      // Try to get the actual content dimensions
      const range = document.createRange();
      range.selectNodeContents(element);
      const contentRect = range.getBoundingClientRect();
      range.detach();
      
      return {
        width: contentRect.width || 0,
        height: contentRect.height || 0
      };
    }
    
    getElementName(element) {
      // Try different ways to get the name from the DOM
      return element.dataset.highlightName || 
             element.getAttribute('data-name') || 
             element.getAttribute('aria-label') || 
             element.title || 
             element.textContent?.trim().substring(0, 30) || 
             element.tagName.toLowerCase();
    }
    
    highlight(element, customName = null) {
      if (typeof element === 'string') {
        element = document.querySelector(element);
      }
      
      if (!element) {
        console.warn('Element not found for highlighting');
        return;
      }
      
      if (this.overlays.has(element)) {
        return; // Already highlighted
      }
      
      const name = customName || this.getElementName(element);
      const overlayData = {
        element: element,
        name: name,
        overlay: null,
        updatePosition: null
      };
      
      this.overlays.set(element, overlayData);
      
      // If we're currently showing overlays, create the DOM immediately
      if (this.isVisible) {
        this.createOverlayDOM(overlayData);
      }
      
      return overlayData;
    }
    
    createOverlayDOM(overlayData) {
      const { element, name } = overlayData;
      const overlay = this.createOverlay(element, name);
      
      // Update position on scroll and resize
      const updatePosition = () => this.updateOverlayPosition(overlay, element);
      window.addEventListener('scroll', updatePosition);
      window.addEventListener('resize', updatePosition);
      
      overlayData.overlay = overlay;
      overlayData.updatePosition = updatePosition;
      
      // Show with animation - double RAF ensures initial styles are painted
      requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          overlay.classList.add('visible');
        });
      });
    }
    
    destroyOverlayDOM(overlayData) {
      const { overlay, updatePosition } = overlayData;
      
      if (!overlay) return;
      
      // Hide with animation
      overlay.classList.remove('visible');
      
      // Wait for animation to complete, then remove from DOM
      setTimeout(() => {
        if (overlay.parentNode) {
          overlay.parentNode.removeChild(overlay);
        }
        if (updatePosition) {
          window.removeEventListener('scroll', updatePosition);
          window.removeEventListener('resize', updatePosition);
        }
        overlayData.overlay = null;
        overlayData.updatePosition = null;
              }, this.options.transitionDuration);
    }
    
    removeHighlight(element) {
      if (typeof element === 'string') {
        element = document.querySelector(element);
      }
      
      const overlayData = this.overlays.get(element);
      if (overlayData) {
        this.destroyOverlayDOM(overlayData);
        this.overlays.delete(element);
      }
    }
    
    show() {
      this.isVisible = true;
      
      // Create DOM for all overlays
      this.overlays.forEach(overlayData => {
        if (!overlayData.overlay) {
          this.createOverlayDOM(overlayData);
        }
      });
    }
    
    hide() {
      this.isVisible = false;
      
      // Destroy DOM for all overlays
      this.overlays.forEach(overlayData => {
        if (overlayData.overlay) {
          this.destroyOverlayDOM(overlayData);
        }
      });
    }
    
    toggle() {
      if (this.isVisible) {
        this.hide();
      } else {
        this.show();
      }
    }
    
    clear() {
      this.overlays.forEach((overlayData, element) => {
        this.destroyOverlayDOM(overlayData);
      });
      this.overlays.clear();
      this.isVisible = false;
    }
    
    highlightMultiple(elements) {
      elements.forEach(element => {
        if (typeof element === 'object' && element.element) {
          // Handle objects with element and name properties
          this.highlight(element.element, element.name);
        } else {
          this.highlight(element);
        }
      });
    }
    
    // Utility method to highlight elements by selector
    highlightBySelector(selector) {
      const elements = document.querySelectorAll(selector);
      elements.forEach(element => this.highlight(element));
    }
    
    // Method to diagnose element layout issues
    diagnoseElement(element) {
      if (typeof element === 'string') {
        element = document.querySelector(element);
      }
      
      if (!element) {
        console.warn('Element not found for diagnosis');
        return null;
      }
      
      const rect = element.getBoundingClientRect();
      const computedStyle = window.getComputedStyle(element);
      const isFlexChild = this.isFlexOrGridChild(element);
      const isEmpty = this.isEmptyElement(element);
      
      const diagnosis = {
        element: element,
        dimensions: {
          width: rect.width,
          height: rect.height,
          top: rect.top,
          left: rect.left
        },
        computed: {
          display: computedStyle.display,
          position: computedStyle.position,
          flexGrow: computedStyle.flexGrow,
          flexShrink: computedStyle.flexShrink,
          flexBasis: computedStyle.flexBasis,
          gridColumn: computedStyle.gridColumn,
          gridRow: computedStyle.gridRow
        },
        context: {
          isFlexChild,
          isEmpty,
          hasParent: !!element.parentElement,
          parentDisplay: element.parentElement ? window.getComputedStyle(element.parentElement).display : null
        },
        issues: []
      };
      
      // Detect potential issues
      if (rect.width < 20 || rect.height < 20) {
        diagnosis.issues.push('Very small dimensions - might be difficult to see');
      }
      
      if (isEmpty && (rect.width === 0 || rect.height === 0)) {
        diagnosis.issues.push('Empty element with zero dimensions');
      }
      
      if (isFlexChild && computedStyle.flexShrink !== '0' && (rect.width < 50 || rect.height < 50)) {
        diagnosis.issues.push('Flex item might be shrunk smaller than expected');
      }
      
      if (rect.width > window.innerWidth * 0.9 || rect.height > window.innerHeight * 0.9) {
        diagnosis.issues.push('Element is very large - might indicate layout issues');
      }
      
      return diagnosis;
    }
  }

/**
 * Slotty Slot Visualization
 * Enables developer mode visualization of slots using HighlightOverlay
 * 
 * Shows slot visualization when Alt + S is pressed
 * Double press Alt + S to toggle permanently
 */
(function() {
    'use strict'
  
    // Configuration
    const KEY_COMBO = {
      alt: true,
      key: 's'
    }
    const STORAGE_KEY = 'slotty-slots-visible'
    const DOUBLE_PRESS_DELAY = 500 // ms
  
    // State
    let isVisible = false
    let isPermanent = false
    let lastKeyPress = 0
    let isKeyDown = false
    let altKeyDown = false
    let sKeyDown = false
    let highlightOverlay = null
    let isHighlighting = false // Prevent recursive highlighting

    // Initialize highlighter
    function initHighlighter() {
      try {
        return new HighlightOverlay({
          // Options will be read from CSS variables
        })
      } catch (error) {
        console.warn('Failed to initialize HighlightOverlay:', error)
        return null
      }
    }

    // Find and highlight slot elements
    function highlightSlots() {
      if (!highlightOverlay || isHighlighting) return

      isHighlighting = true

      // Clear existing highlights
      highlightOverlay.clear()

      // Find all slot elements and highlight them
      const slots = document.querySelectorAll('[data-slotty-slot]')
      
      slots.forEach(slot => {
        const slotName = slot.getAttribute('data-slotty-slot') || 'unnamed-slot'
        
        // Check if slot has meaningful content
        const hasContent = hasSlotContent(slot)
        const isEmpty = !hasContent
        
        // Use slot name and add content indicator
        const displayName = `${slotName}${isEmpty ? ' (empty)' : ''}`
        
        highlightOverlay.highlight(slot, displayName)
      })

      isHighlighting = false
    }

    // Helper function to detect if a slot has meaningful content
    function hasSlotContent(slotElement) {
      // Get the text content, trimmed of whitespace
      const textContent = slotElement.textContent?.trim() || ''
      
      // Check for child elements (excluding script/style elements that might be injected)
      const meaningfulChildren = Array.from(slotElement.children).filter(child => 
        !['SCRIPT', 'STYLE'].includes(child.tagName)
      )
      
      // For display: contents elements, also check if children have meaningful content
      const computedStyle = window.getComputedStyle(slotElement)
      let hasBackgroundImage = computedStyle.backgroundImage !== 'none'
      
      // If display: contents, check children for visual content too
      if (computedStyle.display === 'contents' && meaningfulChildren.length > 0) {
        const hasVisibleChildren = meaningfulChildren.some(child => {
          const childStyle = window.getComputedStyle(child)
          return childStyle.display !== 'none' && 
                 (child.textContent?.trim().length > 0 || childStyle.backgroundImage !== 'none')
        })
        if (hasVisibleChildren) return true
      }
      
      // Consider it to have content if:
      // - Has non-empty text content
      // - Has meaningful child elements 
      // - Has background image
      return textContent.length > 0 || meaningfulChildren.length > 0 || hasBackgroundImage
    }
  
    // DOM manipulation
    function showSlots() {
      if (isVisible) return
      
      if (!highlightOverlay) {
        highlightOverlay = initHighlighter()
        if (!highlightOverlay) return
      }
      
      highlightSlots()
      highlightOverlay.show()
      isVisible = true
    }
  
    function hideSlots() {
      if (!isVisible || isPermanent) return
      
      if (highlightOverlay) {
        highlightOverlay.hide()
      }
      isVisible = false
    }
  
    function togglePermanentVisibility() {
      isPermanent = !isPermanent
      
      if (isPermanent) {
        showSlots()
        sessionStorage.setItem(STORAGE_KEY, 'true')
      } else {
        hideSlots()
        sessionStorage.removeItem(STORAGE_KEY)
      }
    }
  
    // Check for double press
    function isDoublePress() {
      const now = Date.now()
      const isDouble = (now - lastKeyPress) < DOUBLE_PRESS_DELAY
      lastKeyPress = now
      return isDouble
    }
  
    // Event handlers
    function handleKeyDown(event) {
      // Track individual key states
      if (event.key === 'Alt') {
        altKeyDown = true
      }
      if (event.key.toLowerCase() === KEY_COMBO.key) {
        sKeyDown = true
      }
      
      // Trigger when both Alt and S are pressed
      if (altKeyDown && sKeyDown) {
        event.preventDefault()
        
        // Ignore if combo is already active (prevents repeat triggers)
        if (isKeyDown) return
        isKeyDown = true
        
        if (isDoublePress()) {
          togglePermanentVisibility()
        } else {
          showSlots()
        }
      }
    }
  
    function handleKeyUp(event) {
      // Track individual key releases
      if (event.key === 'Alt') {
        altKeyDown = false
      }
      if (event.key.toLowerCase() === KEY_COMBO.key) {
        sKeyDown = false
      }
      
      // Hide when either key is released (and not in permanent mode)
      if (isKeyDown && (!altKeyDown || !sKeyDown)) {
        isKeyDown = false
        if (!isPermanent) {
          hideSlots()
        }
      }
    }

    // Handle dynamic content changes
    function handleContentChange(mutations) {
      if (!isVisible || !highlightOverlay || isHighlighting) return

      // Check if the mutations actually affect slot content (not just overlay changes)
      const relevantChange = mutations.some(mutation => {
        // Ignore changes to our own overlay elements
        if (mutation.target.classList?.contains('slotty-overlay') || 
            mutation.target.classList?.contains('slotty-overlay-name-pill')) {
          return false
        }
        
        // Check if any added/removed nodes are slot-related or within slots
        const hasSlotChanges = Array.from(mutation.addedNodes).concat(Array.from(mutation.removedNodes))
          .some(node => {
            if (node.nodeType !== Node.ELEMENT_NODE) return false
            return node.hasAttribute?.('data-slotty-slot') || 
                   node.closest?.('[data-slotty-slot]')
          })
        
        return hasSlotChanges || mutation.type === 'attributes'
      })

      if (relevantChange) {
        // Re-highlight slots when content changes
        setTimeout(highlightSlots, 100) // Small delay to allow DOM to settle
      }
    }
  
    // Initialize
    function init() {
      // Check session storage for persistent state
      if (sessionStorage.getItem(STORAGE_KEY) === 'true') {
        isPermanent = true
        showSlots()
      }
  
      document.addEventListener('keydown', handleKeyDown)
      document.addEventListener('keyup', handleKeyUp)
      
      // Hide slots when window loses focus (only if not permanent)
      window.addEventListener('blur', () => {
        if (!isPermanent) hideSlots()
        // Reset key state when window loses focus
        isKeyDown = false
      })

      // Listen for DOM changes to re-highlight slots
      if (typeof MutationObserver !== 'undefined') {
        const observer = new MutationObserver(handleContentChange)
        observer.observe(document.body, {
          childList: true,
          subtree: true,
          attributes: true,
          attributeFilter: ['data-slotty-slot', 'data-slot-name']
        })
      }
    }
  
    // Start when DOM is ready
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', init)
    } else {
      init()
    }
  })() 