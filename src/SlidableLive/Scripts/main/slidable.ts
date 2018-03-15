/// <reference path="./notes.ts" />
/// <reference path="./questions.ts" />
/// <reference path="./nav.ts" />
/// <reference path="../hub.ts" />

// ReSharper disable once InconsistentNaming
interface SlideAvailable {
    number: number;
}

namespace Slidable.AutoNav {
    import NotesForm = Notes.NotesForm;
    import QuestionsForm = Questions.QuestionsForm;
    import NavButtons = Nav.NavButtons;

    var notesForm: NotesForm;
    var questionsForm: QuestionsForm;
    var nav: NavButtons;

    Hub.onConnected(() => {
        Hub.hubConnection.on<SlideAvailable>("slideAvailable",
            data => {
                if (data.number) {
                    if (notesForm.dirty || questionsForm.dirty) return;
                    nav.go(window.location.pathname.replace(/\/[0-9]+$/, `/${data.number}`));
                }
            });
    });

    document.addEventListener("DOMContentLoaded", () => {

        notesForm = new NotesForm();
        notesForm.load();

        questionsForm = new QuestionsForm();
        questionsForm.load();

        nav = new NavButtons();

        Hub.connect();
    });
}