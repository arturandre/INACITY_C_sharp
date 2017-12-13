class JobManager {
    constructor() {
        this.jobsQueue = [];
        this.activeJobs = 0;
        this.onCreate = null;
        this.onUpdate = null;
        this.onRemove = null;
    }

    createJob(progress, maxProgress, description, done) {
        if (maxProgress <= progress) {
            console.warn("JobManager - createJob: Tried to create finished job. (progress >= maxProgress)");
            return -1;
        }
        this.activeJobs++;
        var newJob = new Job(progress, maxProgress, description,
            function (e) {
                this.removeJob(newJob.id);
                if (done) {
                    done(e);
                }
            }.bind(this));
        this.jobsQueue[newJob.id] = newJob;
        if (this.onCreate) {
            this.onCreate(this, newJob);
        }
        return newJob.id;
    }

    removeJob(jobid) {

        if (!this.jobsQueue[jobid]) return;

        var deletedJob = this.jobsQueue[jobid];
        delete this.jobsQueue[jobid]
        this.activeJobs--;
        if (this.onRemove) {
            this.onRemove(this, deletedJob);
        }

    }

    updateJobBy(jobid, amount) {
        let value = this.jobsQueue[jobid].progress;
        this.jobsQueue[jobid].setProgress(amount + value);
        if (this.onUpdate && this.jobsQueue[jobid]) {
            this.onUpdate(this, this.jobsQueue[jobid]);
        }
    }

}



