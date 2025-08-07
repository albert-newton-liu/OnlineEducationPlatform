let availableVoices = [];

const populateVoiceList = () => {
    availableVoices = window.speechSynthesis.getVoices();
    console.log("Available voices loaded:", availableVoices);
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

        utterance.rate = rate; // 修正：使用传入的 rate 参数
        utterance.pitch = pitch; // 修正：使用传入的 pitch 参数

        window.speechSynthesis.speak(utterance);
    } else {
        console.error("Browser does not support Web Speech API.");
        alert("Your browser does not support text-to-speech.");
    }
};


// 新增：暂停播放功能
export const pauseSpeech = () => {
    if ('speechSynthesis' in window && window.speechSynthesis.speaking) {
        window.speechSynthesis.pause();
    }
};

// 新增：继续播放功能
export const resumeSpeech = () => {
    if ('speechSynthesis' in window && window.speechSynthesis.paused) {
        window.speechSynthesis.resume();
    }
};

// 新增：停止播放功能
export const cancelSpeech = () => {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel();
    }
};

export const getAvailableVoices = () => availableVoices;