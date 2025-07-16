### Database Table Designs

---

### 1. User Management

#### `Users` Table

| Field Name      | Type      | Description                                                 |
| --------------- | --------- | ----------------------------------------------------------- |
| `user_id`       | String    | **PK**. Unique identifier for the user.                     |
| `username`      | String    | **UNIQUE**, NOT NULL. User's chosen username.               |
| `email`         | String    | **UNIQUE**, NOT NULL. User's email address.                 |
| `password_hash` | String    | NOT NULL. Hashed password for security.                     |
| `role`          | Byte      | NOT NULL. User's role: `0=Student`, `1=Teacher`, `2=Admin`. |
| `created_at`    | Timestamp | DEFAULT CURRENT_TIMESTAMP. Timestamp of user creation.      |
| `last_login_at` | Timestamp | Last time the user logged in.                               |
| `is_active`     | Boolean   | DEFAULT TRUE. Flag for account status (e.g., suspend/ban).  |



#### `Students` Table

| Field Name      | Type    | Description                                          |
| --------------- | ------- | ---------------------------------------------------- |
| `student_id`    | String  | **PK**, **FK** to `Users.user_id`.                   |
| `parent_email`  | String  | Optional: Parent's email for communication/tracking. |
| `date_of_birth` | Date    | Student's date of birth.                             |
| `avatar_url`    | String  | URL to the student's profile picture.                |
| `total_rewards` | Integer | DEFAULT 0. Accumulated reward points or stars.       |



#### `Teachers` Table

| Field Name            | Type     | Description                                        |
| --------------------- | -------- | -------------------------------------------------- |
| `teacher_id`          | String   | **PK**, **FK** to `Users.user_id`.                 |
| `bio`                 | Text     | Teacher's biography or introduction.               |
| `profile_picture_url` | String   | URL to the teacher's profile picture.              |
| `is_approved`         | Boolean  | DEFAULT FALSE. Admin approval status for teachers. |
| `rating`              | Numeric  | Average rating from students (e.g., 4.5).          |
| `teaching_languages`  | String[] | An array of languages the teacher teaches.         |



#### `Admins` Table

| Field Name    | Type     | Description                                      |
| ------------- | -------- | ------------------------------------------------ |
| `admin_id`    | String   | **PK**, **FK** to `Users.user_id`.               |
| `permissions` | String[] | An array of specific administrative permissions. |



---

### 2. Lesson Content Management

#### `Lessons` Table

| Field Name          | Type      | Description                                                          |
| ------------------- | --------- | -------------------------------------------------------------------- |
| `lesson_id`         | String    | **PK**. Unique identifier for the lesson.                            |
| `teacher_id`        | String    | **FK** to `Teachers.teacher_id`, NOT NULL.                           |
| `title`             | String    | NOT NULL. Title of the lesson.                                       |
| `description`       | Text      | Short description of the lesson.                                     |
| `difficulty_level`  | Byte      | Lesson difficulty: `0=Beginner`, `1=Intermediate`, `2=Advanced`.     |
| `thumbnail_url`     | String    | URL to the lesson's cover image/thumbnail for listings.              |
| `created_at`        | Timestamp | DEFAULT CURRENT_TIMESTAMP. When the lesson was created.              |
| `updated_at`        | Timestamp | DEFAULT CURRENT_TIMESTAMP. Last update timestamp.                    |
| `is_published`      | Boolean   | DEFAULT FALSE. Controls visibility to students.                      |
| `admin_reviewed_at` | Timestamp | Timestamp of last admin review (optional, for moderation workflows). |



#### `LessonPages` Table

| Field Name         | Type      | Description                                                                                                                                                                                       |
| ------------------ | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `page_id`          | String    | **PK**. Unique identifier for a lesson page.                                                                                                                                                      |
| `lesson_id`        | String    | **FK** to `Lessons.lesson_id`, NOT NULL.                                                                                                                                                          |
| `page_number`      | Integer   | NOT NULL. Order of the page within the lesson.                                                                                                                                                    |
| `page_layout_json` | JSON      | NOT NULL. **Crucial for layout.** Stores an ordered JSON array of `element_id`s and their layout properties (e.g., `[{"element_id": "...", "type": "text", "order": 1, "alignment": "center"}]`). |
| `created_at`       | Timestamp | DEFAULT CURRENT_TIMESTAMP.                                                                                                                                                                        |
| `updated_at`       | Timestamp | DEFAULT CURRENT_TIMESTAMP.                                                                                                                                                                        |



#### `LessonPageElements` Table

| Field Name      | Type      | Description                                                                                                                                                                     |
| --------------- | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `element_id`    | String    | **PK**. Unique identifier for a page element.                                                                                                                                   |
| `page_id`       | String    | **FK** to `LessonPages.page_id`, NOT NULL.                                                                                                                                      |
| `element_type`  | Byte      | NOT NULL. Type of the content element: `0=Text`, `1=Image`, `2=Audio`, `3=Pronounce_Text`, `4=Video`, `5=Interactive`.                                                          |
| `content_text`  | Text      | Stores textual content (for 'text' type).                                                                                                                                       |
| `content_url`   | String    | Stores URL for media files (for 'image', 'audio', 'video' types).                                                                                                               |
| `metadata_json` | JSON      | Flexible field for type-specific data (e.g., for `pronounce_text`, it stores `{"full_audio_url": "...", "segments": [{"word": "...", "start_time": ..., "end_time": "..."}]}`). |
| `created_at`    | Timestamp | DEFAULT CURRENT_TIMESTAMP.                                                                                                                                                      |



#### `StudentLessonProgress` Table

| Field Name         | Type      | Description                                                              |
| ------------------ | --------- | ------------------------------------------------------------------------ |
| `progress_id`      | String    | **PK**.                                                                  |
| `student_id`       | String    | **FK** to `Students.student_id`, NOT NULL.                               |
| `lesson_id`        | String    | **FK** to `Lessons.lesson_id`, NOT NULL.                                 |
| `last_page_viewed` | Integer   | The highest page number the student has viewed in this lesson.           |
| `completed_at`     | Timestamp | Timestamp if the lesson was completed.                                   |
| `status`           | Byte      | Lesson progress status: `0=Not Started`, `1=In Progress`, `2=Completed`. |



---

### 3. Booking System

#### `TeacherRecurringAvailability` Table

| Field Name            | Type      | Description                                                                                                                                        |
| --------------------- | --------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| `recurring_id`        | String    | **PK**. Unique identifier for this recurring schedule entry.                                                                                       |
| `teacher_id`          | String    | **FK** to `Teachers.teacher_id`, NOT NULL.                                                                                                         |
| `day_of_week`         | Byte      | NOT NULL. The day of the week for this recurring slot: `0=Monday`, `1=Tuesday`, `2=Wednesday`, `3=Thursday`, `4=Friday`, `5=Saturday`, `6=Sunday`. |
| `start_time_of_day`   | Time      | NOT NULL. The start time of day (e.g., 09:00:00).                                                                                                  |
| `end_time_of_day`     | Time      | NOT NULL. The end time of day (e.g., 17:00:00).                                                                                                    |
| `effective_from_date` | Date      | NOT NULL. Date from which this recurring availability is valid.                                                                                    |
| `effective_to_date`   | Date      | Optional. Date until which this recurring availability is valid. Null means indefinite.                                                            |
| `is_active`           | Boolean   | DEFAULT TRUE. Allows teachers to temporarily disable a recurring schedule.                                                                         |
| `created_at`          | Timestamp | DEFAULT CURRENT_TIMESTAMP.                                                                                                                         |
| `updated_at`          | Timestamp | DEFAULT CURRENT_TIMESTAMP.                                                                                                                         |



#### `BookableSlots` Table

| Field Name     | Type      | Description                                                                                                                                        |
| -------------- | --------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| `slot_id`      | String    | **PK**. Unique identifier for the specific bookable slot.                                                                                          |
| `teacher_id`   | String    | **FK** to `Teachers.teacher_id`, NOT NULL.                                                                                                         |
| `start_time`   | Timestamp | NOT NULL. **Exact start time** of the 15-minute slot.                                                                                              |
| `end_time`     | Timestamp | NOT NULL. **Exact end time** of the 15-minute slot (e.g., 15 mins after `start_time`).                                                             |
| `is_booked`    | Boolean   | DEFAULT FALSE. True if this specific slot is booked.                                                                                               |
| `recurring_id` | String    | **FK** to `TeacherRecurringAvailability.recurring_id`. Optional: Links to the recurring rule it was generated from. Can be NULL for one-off slots. |
| `is_one_off`   | Boolean   | DEFAULT FALSE. True if this slot was manually added and not generated from a recurring rule.                                                       |
| `created_at`   | Timestamp | DEFAULT CURRENT_TIMESTAMP.                                                                                                                         |



#### `Bookings` Table

| Field Name     | Type      | Description                                                                                                              |
| -------------- | --------- | ------------------------------------------------------------------------------------------------------------------------ |
| `booking_id`   | String    | **PK**.                                                                                                                  |
| `student_id`   | String    | **FK** to `Students.student_id`, NOT NULL.                                                                               |
| `teacher_id`   | String    | **FK** to `Teachers.teacher_id`, NOT NULL.                                                                               |
| `slot_id`      | String    | **PK**, **FK** to `BookableSlots.slot_id`, **UNIQUE**, NOT NULL. Links directly to the specific bookable 15-minute slot. |
| `lesson_id`    | String    | **FK** to `Lessons.lesson_id`. Optional: If a specific lesson is chosen for the booking.                                 |
| `booking_time` | Timestamp | NOT NULL. The confirmed start time of the booked class (should match `BookableSlots.start_time`).                        |
| `status`       | Byte      | Booking status: `0=Scheduled`, `1=Completed`, `2=Cancelled`, `3=No Show`.                                                |
| `created_at`   | Timestamp | DEFAULT CURRENT_TIMESTAMP.                                                                                               |
| `updated_at`   | Timestamp | DEFAULT CURRENT_TIMESTAMP.                                                                                               |



---

### 4. Live Class

#### `LiveClasses` Table

| Field Name          | Type      | Description                                                                                                           |
| ------------------- | --------- | --------------------------------------------------------------------------------------------------------------------- |
| `live_class_id`     | String    | **PK**.                                                                                                               |
| `booking_id`        | String    | **PK**, **FK** to `Bookings.booking_id`, **UNIQUE**, NOT NULL. A live class is directly tied to one specific booking. |
| `room_name`         | String    | **UNIQUE**, NOT NULL. Randomly generated room identifier for video conferencing (e.g., Jitsi Meet).                   |
| `room_password`     | String    | Optional password for additional room security.                                                                       |
| `start_time`        | Timestamp | NOT NULL. Actual start time of the class.                                                                             |
| `end_time`          | Timestamp | Actual end time of the class.                                                                                         |
| `teacher_join_time` | Timestamp | Timestamp when teacher joined the room.                                                                               |
| `student_join_time` | Timestamp | Timestamp when student joined the room.                                                                               |



---

### 5. Rewards System

#### `Rewards` Table

| Field Name     | Type      | Description                                                                  |
| -------------- | --------- | ---------------------------------------------------------------------------- |
| `reward_id`    | String    | **PK**.                                                                      |
| `student_id`   | String    | **FK** to `Students.student_id`, NOT NULL.                                   |
| `teacher_id`   | String    | **FK** to `Teachers.teacher_id`, NOT NULL.                                   |
| `reward_type`  | Byte      | NOT NULL. Type of reward given: `0=Star`, `1=Badge`.                         |
| `reward_name`  | String    | Specific name of a badge (e.g., "Super Reader Badge").                       |
| `reward_value` | Integer   | Number of stars or points awarded.                                           |
| `awarded_at`   | Timestamp | DEFAULT CURRENT_TIMESTAMP. When the reward was given.                        |
| `lesson_id`    | String    | **FK** to `Lessons.lesson_id`. Optional: lesson where the reward was earned. |



---

### 6. Notifications

#### `Notifications` Table

| Field Name            | Type      | Description                                                                                                                                                                                                                |
| --------------------- | --------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `notification_id`     | String    | **PK**.                                                                                                                                                                                                                    |
| `user_id`             | String    | **FK** to `Users.user_id`, NOT NULL. The recipient of the notification.                                                                                                                                                    |
| `type`                | Byte      | NOT NULL. Type of the notification: `0=Booking Confirmation`, `1=Booking Reminder`, `2=Lesson Update`, `3=Admin Message`, `4=Reward Received`, `5=Teacher Approved`, `6=Slot Cancelled`. You can add more types as needed. |
| `message`             | Text      | NOT NULL. The content of the notification.                                                                                                                                                                                 |
| `is_read`             | Boolean   | DEFAULT FALSE. Whether the user has read the notification.                                                                                                                                                                 |
| `created_at`          | Timestamp | DEFAULT CURRENT_TIMESTAMP.                                                                                                                                                                                                 |
| `related_entity_id`   | String    | Optional: ID of the record this notification relates to (e.g., `booking_id`, `lesson_id`). Helps in deep-linking or contextualizing the notification.                                                                      |
| `related_entity_type` | String    | Optional: Type of the related entity (e.g., 'Booking', 'Lesson'). Useful when `related_entity_id` could reference IDs from different tables.                                                                               |


