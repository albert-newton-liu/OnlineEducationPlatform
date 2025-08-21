import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { API_BASE_URL } from '../../../../constant/Constants';
import './ScheduleManagementPage.css';

// Define business hours and slot duration
const START_HOUR = 9; // 9:00 AM
const END_HOUR = 17; // 5:00 PM
const SLOT_DURATION_MINUTES = 25;
const DAYS_OF_WEEK = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'];

// Helper function to generate all possible time slots for a day
const generateTimeSlots = () => {
    const slots = [];
    for (let h = START_HOUR; h < END_HOUR; h++) {
        for (let m = 0; m < 60; m += 30) {
            const start = new Date();
            start.setHours(h, m, 0, 0);

            const end = new Date(start);
            end.setMinutes(start.getMinutes() + SLOT_DURATION_MINUTES);
            
            slots.push({
                startTime: start.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }),
                endTime: end.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
            });
        }
    }
    return slots;
};

// All available time slots
const timeSlots = generateTimeSlots();

// Helper to get dayOfWeek from a day name
const getDayOfWeek = (dayName) => DAYS_OF_WEEK.indexOf(dayName);

// Helper to get day name from dayOfWeek
const getDayName = (dayOfWeek) => DAYS_OF_WEEK[dayOfWeek];

// Helper to convert 'h:mm AM/PM' to 'HH:mm:ss' for C# TimeSpan
const convertToTimeSpanFormat = (timeString) => {
    const [time, period] = timeString.split(' ');
    let [hours, minutes] = time.split(':');
    
    hours = parseInt(hours);
    
    if (period === 'PM' && hours !== 12) {
        hours += 12;
    } else if (period === 'AM' && hours === 12) {
        hours = 0; // Midnight case
    }
    
    const formattedHours = String(hours).padStart(2, '0');
    const formattedMinutes = String(minutes).padStart(2, '0');
    
    return `${formattedHours}:${formattedMinutes}:00`;
};

export default function ScheduleManagementPage() {
    const [schedule, setSchedule] = useState({});
    const [isEditing, setIsEditing] = useState(false);
    const [hasChanged, setHasChanged] = useState(false);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const teacherId = localStorage.getItem('userId');

    const fetchSchedule = async () => {
        if (!teacherId) {
            setError('Teacher ID not found. Please log in again.');
            setLoading(false);
            return;
        }

        try {
            setLoading(true);
            const response = await axios.get(`${API_BASE_URL}/api/Booking/getSchedule/${teacherId}`);
            
            const apiSchedule = response.data;
            if (apiSchedule && apiSchedule.teacherDaySchedules) {
                const newSchedule = {};
                apiSchedule.teacherDaySchedules.forEach(daySchedule => {
                    const dayName = getDayName(daySchedule.dayOfWeek);
                    if (dayName) {
                        // The backend may return 'HH:mm:ss' which needs to be converted back
                        // to 'h:mm AM/PM' for display.
                        newSchedule[dayName] = daySchedule.duarations.map(d => {
                             // Assuming backend returns HH:mm:ss, convert to h:mm AM/PM
                            const [hours, minutes] = d.startTime.split(':');
                            const date = new Date();
                            date.setHours(hours, minutes, 0, 0);
                            const formattedStartTime = date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });

                            const [endHours, endMinutes] = d.endTime.split(':');
                            const endDate = new Date();
                            endDate.setHours(endHours, endMinutes, 0, 0);
                            const formattedEndTime = endDate.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });

                            return `${formattedStartTime} - ${formattedEndTime}`;
                        });
                    }
                });
                setSchedule(newSchedule);
            }
        } catch (err) {
            console.error('Failed to fetch schedule:', err);
            setError('Failed to load schedule. Please try again.');
            setSchedule({});
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchSchedule();
    }, []);

    const handleSlotClick = (day, slot) => {
        if (!isEditing) return;

        const slotString = `${slot.startTime} - ${slot.endTime}`;
        const newSchedule = { ...schedule };
        const daySlots = newSchedule[day] || [];
        
        if (daySlots.includes(slotString)) {
            newSchedule[day] = daySlots.filter(s => s !== slotString);
        } else {
            newSchedule[day] = [...daySlots, slotString];
        }
        setSchedule(newSchedule);
        setHasChanged(true);
    };

    const handleEditClick = () => {
        setIsEditing(true);
    };

    const handleSaveClick = async () => {
        if (!hasChanged) return;

        // Transform local state to API request body format and sort
        const requestBody = {
            teacherId: teacherId,
            teacherDaySchedules: Object.keys(schedule).map(dayName => {
                // Sort duarations by start time before mapping
                const sortedDuarations = [...schedule[dayName]].sort((a, b) => {
                    const [startTimeA] = a.split(' - ');
                    const [startTimeB] = b.split(' - ');
                    return new Date(`1/1/2025 ${startTimeA}`) - new Date(`1/1/2025 ${startTimeB}`);
                });

                return {
                    dayOfWeek: getDayOfWeek(dayName),
                    duarations: sortedDuarations.map(slotString => {
                        const [startTime, endTime] = slotString.split(' - ');
                        return {
                            startTime: convertToTimeSpanFormat(startTime),
                            endTime: convertToTimeSpanFormat(endTime)
                        };
                    })
                };
            })
        };

        try {
            await axios.post(`${API_BASE_URL}/api/Booking/addSchedule`, requestBody);
            alert("Schedule saved successfully!");
            setHasChanged(false);
            setIsEditing(false);
        } catch (err) {
            console.error("Failed to save schedule:", err);
            alert("Failed to save schedule. Please try again.");
        }
    };

    if (loading) {
        return <div className="schedule-message">Loading schedule...</div>;
    }

    if (error) {
        return <div className="schedule-message error-message">{error}</div>;
    }

    return (
        <div className="schedule-management-container">
            <div className="schedule-header">
                <h2>Schedule Management</h2>
                <div className="schedule-actions">
                    {!isEditing && (
                        <button className="edit-button" onClick={handleEditClick}>Edit</button>
                    )}
                    {isEditing && (
                        <>
                            <button
                                className="save-button"
                                onClick={handleSaveClick}
                                disabled={!hasChanged}
                            >
                                Save
                            </button>
                            <button className="cancel-button" onClick={() => {
                                setIsEditing(false);
                                setHasChanged(false);
                                fetchSchedule(); // Re-fetch to discard changes
                            }}>Cancel</button>
                        </>
                    )}
                </div>
            </div>

            <div className="schedule-grid">
                <div className="grid-cell day-of-week header-cell empty-cell"></div>
                {DAYS_OF_WEEK.map(day => (
                    <div key={day} className="grid-cell day-of-week header-cell">{day}</div>
                ))}
                
                {timeSlots.map(slot => (
                    <React.Fragment key={slot.startTime}>
                        <div className="grid-cell time-label header-cell">
                            {slot.startTime}
                        </div>
                        
                        {DAYS_OF_WEEK.map(day => {
                            const isSelected = (schedule[day] || []).includes(`${slot.startTime} - ${slot.endTime}`);
                            return (
                                <div
                                    key={day}
                                    className={`grid-cell slot-cell ${isSelected ? 'selected' : ''} ${isEditing ? 'editable' : ''}`}
                                    onClick={() => handleSlotClick(day, slot)}
                                ></div>
                            );
                        })}
                    </React.Fragment>
                ))}
            </div>
        </div>
    );
}