import React, { useState, useEffect, useCallback } from 'react';
import { getAvailableVoices, cancelSpeech } from './TextToSpeechService';
import './VoiceSelector.css'

const VoiceSelector = ({ onVoiceChange, onParamsChange }) => {
    const [voices, setVoices] = useState([]);
    const [selectedVoice, setSelectedVoice] = useState(null);
    const [rate, setRate] = useState(1);   
    const [pitch, setPitch] = useState(1);

    useEffect(() => {
        const allVoices = getAvailableVoices();
        const englishVoices = allVoices.filter(voice =>
            voice.lang.startsWith('en-')
        );

        setVoices(englishVoices);

        if (englishVoices.length > 0) {
            const hit = englishVoices.filter(voice => voice.name === "Google US English")[0]
            setSelectedVoice(hit);
            onVoiceChange(hit);
        }

        const handleVoicesChanged = () => {
            const updatedVoices = getAvailableVoices();
            setVoices(updatedVoices);
            if (updatedVoices.length > 0) {
                setSelectedVoice(updatedVoices[0]);
                onVoiceChange(updatedVoices[0]);
            }
        };

        window.speechSynthesis.addEventListener('voiceschanged', handleVoicesChanged);

        return () => {
            window.speechSynthesis.removeEventListener('voiceschanged', handleVoicesChanged);
        };
    }, [onVoiceChange]);

    const handleSelectChange = (e) => {
        const selectedName = e.target.value;
        const voice = voices.find(v => v.name === selectedName);
        setSelectedVoice(voice);
        onVoiceChange(voice);
    };

    const handleRateChange = (e) => {
        const newRate = parseFloat(e.target.value);
        setRate(newRate);
        onParamsChange({ rate: newRate, pitch });
    }

    const handlePitchChange = (e) => {
        const newPitch = parseFloat(e.target.value);
        setPitch(newPitch);
        onParamsChange({ rate, pitch: newPitch });
    }


    const stopSpeack = useCallback(() => {
        cancelSpeech()
    })


    return (
        <div className="voice-selector-container">
            <span className="voice-selector-label">Choose Voice:</span>
            <select className="voice-selector-dropdown" onChange={handleSelectChange} value={selectedVoice?.name || ''}>
                {voices.length === 0 ? (
                    <option>Loading voices...</option>
                ) : (
                    voices.map(voice => (
                        <option key={voice.name} value={voice.name}>
                            {voice.name} ({voice.lang})
                        </option>
                    ))
                )}
            </select>
            <div className="speech-params-controls">
                <span>Rate: {rate.toFixed(1)}</span>
                <input 
                    type="range" 
                    min="0.5" 
                    max="2" 
                    step="0.1" 
                    value={rate} 
                    onChange={handleRateChange} 
                />
                <span>Pitch: {pitch.toFixed(1)}</span>
                <input 
                    type="range" 
                    min="0" 
                    max="2" 
                    step="0.1" 
                    value={pitch} 
                    onChange={handlePitchChange} 
                />
            </div>
            <button onClick={stopSpeack} className="stop-button">Stop</button>
        </div>
    );
};

export default VoiceSelector;
