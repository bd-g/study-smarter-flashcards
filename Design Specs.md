# Study Smarter Flashcards

### Summary

##### Description
A free-standing application for studying flashcards that makes the process of creating flashcards easier through the ability to upload terms from your notes.

##### Background
My wife is in nursing school, and she loves flashcards. She writes very thorough, structured notes during class, and then goes through afterwards and hand types them into quizlet. Creating the flashcards takes just as much time as studying them. She asked if I could help her out, and I had the idea of a programatically creating the flashcards. I tried to use Quizlet, but they have a closed API, so I decided to create my own flashcard application.

### Goals

##### Goals for v1

- Create a flashcard window that allows for flipping through two-sided flashcards
- Have a shuffle mode that can be turned on and off
- Allow for saving sets with a name and possibly a description
- For v1, manual creation of flashcards through a seperate page. Sets will be immutable at first


##### Goals for future versions
- Export flashcards as CSV or like structure that you can email to others for their import
- Making additions, edits, and deletions to existing sets
- Support automatic flashcard uploads for various file types
  - Word files
  - Excel files
  - OneNote files - is this possible?
  - Google doc/sheets links
- Possibly create a quizlet like structure where it checks off the ones you got correct and just gives you back the ones you messed up on
- Animations for flipping cards or moving between pages
- Fuzzy matching for fill in the blank versions
- Reminders to study certain sets based on memory research of good intervals
- Support for math symbols and formula support

##### Possible Add-ons
- Different modes, like fill in the blank or matching - games ranked highly on the survey

##### Non-goals
- A mobile app or version. That would be a seperate project
- Syncing progress or sets across multiple machines. Could be added if we ever got around to making an app


### Engineering Plan

##### Platform

This application will be built using UWP so that it can be easily released on the Microsoft Store.

##### General Design

I will use the MVVM design pattern.

- Models
  - Each implements INotifyProperty/INotifyCollectionChanged and has overriden Equals and HashCode methods
  - ```IndividualCardModel```
    - Term
    - Definition
    - CardID - GUID
    - bool IsLearned
    - bool IsArchived
  - ```CardSetModel```
    - Set Name
    - Set Description
    - Collection of IndividualCardModels
    - Progress property - number of cards learned
    - Number of times viewed
    - DateTime WhenCreated
    - DateTime LastOpened
    - Boolean IsArchived
- View Models
  - ```MainMenuViewModel```
    - Contains logic for main menu, presenting all the different sets in order that they were last opened
  - ```SetViewModel```
    - Shows set with all its card, with add, edit, and delete options, including adding cards from set to other sets or vice versa
    - Has import button to import flashcards from a file or another format
    - Show options to return to menu or flip through the flash cards (future functionality to play quizzes or games)
    - Also handles creation of new sets - same as edit window
- ```StudyViewModel```
    - Contains logic for all study modes
    - Has the set being used, as well as properties for which study mode you are in
    - ```BasicStudy```
      - Mode for basic flipping of cards
      - Allows for shuffled or non shuffled cards
      - Mark cards as learned to remove them from deck
      - Progress bar at the bottom
    - ```MatchStudy```
      - Matching - for future release
    - ```QuizStudy```
      - Quiz - for future release
   - ```SettingsViewModel```
    - Contains ApplicationDataContainer for local settings
    - Text Size
    - Fuzzy matching with fill in the blank answers (v2 feature)
  - ```FeedbackViewModel```
    - View Model for Page that allows user feedback
- Views
  - ```MainMenuPage```
    - Main Menu layout. Shows each set in a vertical list in order of when last used, potentially with a sort bar as well for sorting based on when last opened, alphabetical, when last used, etc. Archived sets are always at the bottom and are slightly greyed out.
    - Also a button to add a new set - handled in SetView
  - ```SetPage```
    - Shows cards in a grid like fashion with terms above and definitions below
    - Import button with expansion for adding a file or a link to a google doc or something else
    - Buttons on the side for adding, editing, and deleting
      - Adding button has three popup options - add new card, add card from this set to another set, add card from another set to this set
  - ```BasicStudyPage```
    - Shows card in middle, with back and forward buttons, toggle for shuffle mode, and button to mark card as learned. Clicking on card flips it. Main menu button, as well as edit and add (to other sets) buttons
    - Allows for shuffled or non shuffled cards
    - Mark cards as learned to remove them from deck
    - Progress bar at the bottom
  - ```MatchStudyPage```
    - Matching - for future release
  - ```QuizStudyPage```
      - Quiz - for future release
  - ```SettingsPage```
    - Options for Text Size and Fuzzy Matching
  - ```FeedbackPage```
    - Either a custom form that sends an email to me, or somehow integrate with Microsoft Store Reviews

- Messaging
  - Use ```Galasoft.MvvmLight.Messaging.Messenger```
  - Different notification messages that implement MessageBase
    - ```EditSetStatusMessage```
      - Guid of set to archive
      - enum indicating whether to activate, archive, or delete a set
    - ```EditSetMessage```
      - Guid of set to edit - if null, add a new set
      - List of card guids and their new data values
### Security, Privacy, and Risks

I don't plan on ensuring any sort of intense security over the sets or individual cards, authentication will not be required. That could be an add-on for a future release, more for me to learn than for any very practical use.

### Other Approaches (and why they weren't included)

Will complete this section when survey results are more conclusive.