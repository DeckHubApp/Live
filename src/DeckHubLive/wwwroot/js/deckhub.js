var DeckHub;
(function (DeckHub) {
    var Nav;
    (function (Nav) {
        function loadSlide(url) {
            return fetch(url, { method: "GET" })
                .then(response => response.ok ? response.text() : null);
        }
        class NavButtons {
            constructor() {
                this.transition = () => {
                    const url = window.location.href + "/partial";
                    loadSlide(url)
                        .then(json => {
                        if (!json) {
                            this.goBack();
                            return;
                        }
                        const partial = JSON.parse(json);
                        this._image.style.backgroundImage = `url(${partial.slideImageUrl})`;
                    });
                };
                this.go = (href) => {
                    history.pushState(null, null, href);
                    this.transition();
                };
                this.goBack = () => {
                    const parts = location.pathname.split("/");
                    const slide = Math.max(parseInt(parts.pop()) - 1, 0);
                    const href = location.href.replace(/\/[0-9]+$/, `/${slide}`);
                    this.go(href);
                };
                this._onFirst = () => {
                    const current = this._currentSlide();
                    if (current.slide === 0)
                        return;
                    const href = `/live/${current.presenter}/${current.slug}/0`;
                    this.go(href);
                };
                this._onLast = () => {
                    const current = this._currentSlide();
                    const href = `/live/${current.presenter}/${current.slug}`;
                    history.pushState(null, null, href);
                    window.location.reload();
                };
                this._onPrevious = () => {
                    const current = this._currentSlide();
                    if (current.slide === 0)
                        return;
                    const href = `/live/${current.presenter}/${current.slug}/${current.slide - 1}`;
                    this.go(href);
                };
                this._onNext = () => {
                    const current = this._currentSlide();
                    const href = `/live/${current.presenter}/${current.slug}/${current.slide + 1}`;
                    this.go(href);
                };
                this._currentSlide = () => {
                    const parts = location.pathname.split("/");
                    const slide = parseInt(parts.pop());
                    const slug = parts.pop();
                    const presenter = parts.pop();
                    return { presenter, slug, slide };
                };
                this._first = document.querySelector("button#first-btn");
                this._first.addEventListener("click", this._onFirst);
                this._previous = document.querySelector("button#previous-btn");
                this._previous.addEventListener("click", this._onPrevious);
                this._next = document.querySelector("button#next-btn");
                this._next.addEventListener("click", this._onNext);
                this._last = document.querySelector("button#last-btn");
                this._last.addEventListener("click", this._onLast);
                this._image = document.querySelector("div#slide-image");
                window.addEventListener("popstate", this.transition);
            }
        }
        Nav.NavButtons = NavButtons;
    })(Nav = DeckHub.Nav || (DeckHub.Nav = {}));
})(DeckHub || (DeckHub = {}));
var DeckHub;
(function (DeckHub) {
    var Notes;
    (function (Notes) {
        class NotesForm {
            constructor() {
                this._autoSave = () => {
                    if (this._autosaveTimeout) {
                        clearTimeout(this._autosaveTimeout);
                    }
                    this._autosaveTimeout = setTimeout(() => {
                        this.save()
                            .then(saved => {
                            if (!saved) {
                                this._autoSave();
                            }
                        });
                    }, 2000);
                };
                this.load = () => {
                    if (!this._textarea)
                        return;
                    fetch(this.notesUrl, { method: "GET", credentials: "same-origin" })
                        .then(r => r.text())
                        .then(t => {
                        const note = JSON.parse(t);
                        this._textarea.value = note.text;
                    });
                };
                this.save = () => {
                    if (!this._textarea || !this._setSaving(true))
                        return Promise.resolve(false);
                    const notes = this._textarea.value;
                    const json = JSON.stringify({ text: notes });
                    const headers = new Headers();
                    headers.append("Content-Type", "application/json");
                    headers.append("Content-Length", json.length.toString());
                    return fetch(this.notesUrl, { method: "PUT", credentials: "same-origin", body: json, headers: headers })
                        .then(r => {
                        this._setSaving(false);
                        return true;
                    })
                        .catch(r => {
                        console.log(r);
                        this._setSaving(false);
                        return false;
                    });
                };
                this._setSaving = (value) => {
                    if (this._saving && value)
                        return false;
                    this._saving = this._button.disabled = value;
                    if (value) {
                        this._button.classList.add("disabled");
                    }
                    else {
                        this._button.classList.remove("disabled");
                    }
                    return this._saving;
                };
                this._form = document.getElementById("notes");
                if (!!this._form) {
                    this._textarea = this._form.querySelector("textarea");
                    this._button = this._form.querySelector("button");
                    window.addEventListener("popstate", this.load);
                    if (this._textarea) {
                        this._textarea.addEventListener("keyup", this._autoSave);
                        this._textarea.addEventListener("paste", this._autoSave);
                        this._textarea.addEventListener("focus", () => this.dirty = true);
                        this._textarea.addEventListener("blur", () => this.dirty = false);
                    }
                }
            }
            get notesUrl() {
                const path = window.location.pathname.split("/");
                const slideNumber = path.pop();
                const slug = path.pop();
                const presenter = path.pop();
                return `/api/notes/${presenter}/${slug}/${slideNumber}`;
            }
        }
        Notes.NotesForm = NotesForm;
    })(Notes = DeckHub.Notes || (DeckHub.Notes = {}));
})(DeckHub || (DeckHub = {}));
/// <reference path="../hub.ts" />
var DeckHub;
(function (DeckHub) {
    var Questions;
    (function (Questions) {
        // ReSharper restore InconsistentNaming
        class QuestionsForm {
            constructor() {
                this._onConnected = () => {
                    DeckHub.Hub.hubConnection.on("question", q => {
                        this._appendQuestion(q.id, q.user, q.text);
                    });
                };
                this.load = () => {
                    if (!this._list)
                        return;
                    while (this._list.hasChildNodes()) {
                        this._list.removeChild(this._list.lastChild);
                    }
                    fetch(this.questionsUrl, { method: "GET", credentials: "same-origin" })
                        .then(r => r.text())
                        .then(t => {
                        const questions = JSON.parse(t);
                        for (const q of questions) {
                            this._appendQuestion(q.id, q.user, q.text);
                        }
                    });
                };
                this.save = () => {
                    if (!this._textarea || !this._setSaving(true))
                        return Promise.resolve(false);
                    const question = this._textarea.value;
                    const json = JSON.stringify({ text: question });
                    const headers = new Headers();
                    headers.append("Content-Type", "application/json");
                    headers.append("Content-Length", json.length.toString());
                    return fetch(this.questionsUrl, { method: "POST", credentials: "same-origin", body: json, headers: headers })
                        .then(_ => {
                        this._setSaving(false);
                        this.dirty = false;
                        return true;
                    })
                        .catch(r => {
                        console.log(r);
                        this._setSaving(false);
                        return false;
                    });
                };
                this._appendQuestion = (id, user, text) => {
                    id = `q${id}`;
                    let li = this._list.querySelector(`#${id}`);
                    if (!li) {
                        li = document.createElement("li");
                        li.id = id;
                        li.className = "list-group-item";
                        li.innerHTML =
                            `<span class="small"><strong>${user}</strong></span><br><span>${text}</span>`;
                        this._list.appendChild(li);
                    }
                };
                this._onSubmit = (e) => {
                    e.preventDefault();
                    if (!!this._textarea.value) {
                        this.save();
                        this._textarea.value = null;
                    }
                };
                this._setSaving = (value) => {
                    if (this._saving && value)
                        return false;
                    this._saving = this._button.disabled = value;
                    if (value) {
                        this._button.classList.add("disabled");
                    }
                    else {
                        this._button.classList.remove("disabled");
                    }
                    return this._saving;
                };
                DeckHub.Hub.onConnected(this._onConnected);
                this._form = document.getElementById("questions");
                if (!!this._form) {
                    this._textarea = this._form.querySelector("textarea");
                    this._button = this._form.querySelector("button");
                    this._list = this._form.querySelector("ul#question-list");
                    window.addEventListener("popstate", this.load);
                    this._form.addEventListener("submit", this._onSubmit);
                    if (this._textarea) {
                        this._textarea.addEventListener("keyup", () => this.dirty = true);
                        this._textarea.addEventListener("paste", () => this.dirty = true);
                        this._textarea.addEventListener("focus", () => this.dirty = true);
                    }
                }
            }
            get questionsUrl() {
                const path = window.location.pathname.split("/");
                const slideNumber = path.pop();
                const slug = path.pop();
                const presenter = path.pop();
                return `/api/questions/${presenter}/${slug}/${slideNumber}`;
            }
        }
        Questions.QuestionsForm = QuestionsForm;
    })(Questions = DeckHub.Questions || (DeckHub.Questions = {}));
})(DeckHub || (DeckHub = {}));
/// <reference path="./notes.ts" />
/// <reference path="./questions.ts" />
/// <reference path="./nav.ts" />
/// <reference path="../hub.ts" />
var DeckHub;
(function (DeckHub) {
    var AutoNav;
    (function (AutoNav) {
        var NotesForm = DeckHub.Notes.NotesForm;
        var QuestionsForm = DeckHub.Questions.QuestionsForm;
        var NavButtons = DeckHub.Nav.NavButtons;
        var notesForm;
        var questionsForm;
        var nav;
        DeckHub.Hub.onConnected(() => {
            DeckHub.Hub.hubConnection.on("slideAvailable", data => {
                if (data.number) {
                    if (notesForm.dirty || questionsForm.dirty)
                        return;
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
            DeckHub.Hub.connect();
        });
    })(AutoNav = DeckHub.AutoNav || (DeckHub.AutoNav = {}));
})(DeckHub || (DeckHub = {}));
//# sourceMappingURL=deckhub.js.map