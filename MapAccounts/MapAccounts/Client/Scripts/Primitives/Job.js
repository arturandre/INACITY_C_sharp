class Job {
    constructor(progress, maxProgress, description, doneCallback)
    {
        Job.jobsCounter++;
        this._id = Job.jobsCounter;
        this._progress = progress || 0;
        this._maxProgress = maxProgress || 100;
        this._description = description;
        this._done = doneCallback;
    }

    setProgress(newProgress) {
        if (this.isDone)
        {
            console.warn("Tried to change progress in a finished job.")
            return;
        }
        this._progress = newProgress;
        if (newProgress >= this.maxProgress) {
            if (this._done) {
                this._done(this);
            }
        }
    }

    get id() { return this._id; }
    get progress() { return this._progress; }
    get maxProgress() { return this._maxProgress; }
    get description() { return this._description; }
    get isDone() { return this._maxProgress <= this._progress; }
}

Job.jobsCounter = 0;