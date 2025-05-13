/**
 * Slotty Slot Visualization
 * Enables developer mode visualization of slots
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
  
    // DOM manipulation
    function showSlots() {
      if (isVisible) return
      document.documentElement.classList.add('slots-visible')
      isVisible = true
    }
  
    function hideSlots() {
      if (!isVisible || isPermanent) return
      document.documentElement.classList.remove('slots-visible')
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
      if (event.altKey && event.key.toLowerCase() === KEY_COMBO.key) {
        event.preventDefault()
        
        // Ignore if key is already down (prevents repeat triggers)
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
      if (event.key.toLowerCase() === KEY_COMBO.key) {
        isKeyDown = false
      }
      
      if (!isPermanent && (event.key === 'Alt' || (event.altKey && event.key.toLowerCase() === KEY_COMBO.key))) {
        hideSlots()
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
    }
  
    // Start when DOM is ready
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', init)
    } else {
      init()
    }
  })() 