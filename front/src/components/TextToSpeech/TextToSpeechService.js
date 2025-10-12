let availableVoices = [];

const populateVoiceList = () => {
    availableVoices = window.speechSynthesis.getVoices();
    // console.log("Available voices loaded:", availableVoices);
};

if ('speechSynthesis' in window) {
    populateVoiceList();
    window.speechSynthesis.addEventListener('voiceschanged', populateVoiceList);
}

const getVoiceByLang = (lang) => {
    const voice = availableVoices.find(v => v.lang === lang && !v.name.includes("Google"));
    return voice || availableVoices.find(v => v.lang === lang) || null;
};

export const handleTextToSpeech = (text, voice = null, rate = 1, pitch = 1) => {
    if ('speechSynthesis' in window) {
        if (window.speechSynthesis.speaking) {
            window.speechSynthesis.cancel();
        }

        const utterance = new SpeechSynthesisUtterance(text);

        if (voice) {
            utterance.voice = voice;
            utterance.lang = voice.lang;
        }

        utterance.rate = rate; 
        utterance.pitch = pitch; 

        window.speechSynthesis.speak(utterance);
    } else {
        console.error("Browser does not support Web Speech API.");
        alert("Your browser does not support text-to-speech.");
    }
};


export const pauseSpeech = () => {
    if ('speechSynthesis' in window && window.speechSynthesis.speaking) {
        window.speechSynthesis.pause();
    }
};


export const resumeSpeech = () => {
    if ('speechSynthesis' in window && window.speechSynthesis.paused) {
        window.speechSynthesis.resume();
    }
};


export const cancelSpeech = () => {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel();
    }
};

export const getAvailableVoices = () => availableVoices;