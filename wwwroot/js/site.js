

$(document).ready(function () {
    console.log("HabitTracker loaded");
    if ($('#habitsTableBody').length) loadHabits();
});

// LOAD HABITS - Simple version
function loadHabits() {
    $.get("/Habits/GetHabits", function (habits) {
        displayHabits(habits);
    }).fail(function () {
        $('#habitsTableBody').html('<tr><td colspan="5" class="text-center text-danger">Failed to load habits.</td></tr>');
    });
}

// DISPLAY HABITS - Simple version
function displayHabits(habits) {
    const tableBody = $('#habitsTableBody');
    tableBody.empty();

    if (habits.length === 0) {
        tableBody.html('<tr><td colspan="5" class="text-center py-4">No habits found.</td></tr>');
        return;
    }

    habits.forEach(function (habit) {
        const statusBadge = habit.isCompleted
            ? '<span class="badge bg-success">Completed</span>'
            : '<span class="badge bg-warning">Pending</span>';

        const progressBar = '<div class="progress" style="height:10px;"><div class="progress-bar" style="width:' + habit.progress + '%"></div></div><small>' + habit.progress + '%</small>';

        const row = '<tr><td>' + habit.name + '</td><td><span class="badge bg-secondary">' + habit.frequency + '</span></td><td>' + progressBar + '</td><td>' + statusBadge + '</td><td><button class="btn btn-sm btn-success" onclick="markComplete(' + habit.id + ')"><i class="bi bi-check"></i></button> <button class="btn btn-sm btn-danger" onclick="deleteHabit(' + habit.id + ')"><i class="bi bi-trash"></i></button></td></tr>';

        tableBody.append(row);
    });
}

// MARK COMPLETE - Simple version
function markComplete(id) {
    if (confirm("Mark this habit as completed?")) {
        $.post("/Habits/MarkComplete", { id: id }, function (response) {
            if (response.success) {
                alert("Habit marked as complete!");
                loadHabits();
            } else {
                alert("Error: " + response.message);
            }
        }).fail(function () {
            alert("Network error!");
        });
    }
}

// DELETE HABIT - Simple version
function deleteHabit(id) {
    if (confirm("Delete this habit?")) {
        $.post("/Habits/Delete", { id: id }, function (response) {
            if (response.success) {
                alert("Habit deleted!");
                loadHabits();
            } else {
                alert("Error: " + response.message);
            }
        }).fail(function () {
            alert("Network error!");
        });
    }
}

// UPDATE PROGRESS - Simple version
function updateProgress(id, progress) {
    $.post("/Habits/UpdateProgress", { id: id, progress: progress }, function (response) {
        if (response.success) {
            alert(response.message);
            loadHabits();
        } else {
            alert("Error: " + response.message);
        }
    }).fail(function () {
        alert("Network error!");
    });
}

// Simple message function
function showMessage(message, isSuccess) {
    if (isSuccess) {
        alert("✓ " + message);
    } else {
        alert("✗ " + message);
    }
}
