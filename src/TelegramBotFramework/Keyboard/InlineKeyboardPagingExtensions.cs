#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Extension methods for InlineKeyboardBuilder – pagination helpers
// =============================================================================

using System;

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Provides helper methods that extend <see cref="InlineKeyboardBuilder"/> with
/// common pagination button rows (previous / next / page counter).
/// </summary>
public static class InlineKeyboardPagingExtensions
{
    /// <summary>
    /// Adds a pagination row to the builder consisting of optional "Prev" and "Next"
    /// buttons together with a non‑interactive counter label (e.g. <c>Page 2/5</c>).
    /// The method automatically starts a new row before adding the controls.
    /// </summary>
    /// <param name="builder">The <see cref="InlineKeyboardBuilder"/> to extend.</param>
    /// <param name="currentPage">
    /// The currently displayed page (1‑based). Must be between <c>1</c> and <paramref name="totalPages"/>.
    /// </param>
    /// <param name="totalPages">
    /// The total number of pages. Must be at least <c>1</c>.
    /// </param>
    /// <param name="callbackPrefix">
    /// Prefix used to generate callback data for the navigation buttons.
    /// The method appends <c>_prev</c> or <c>_next</c> to this prefix.
    /// </param>
    /// <returns>The same builder instance, allowing further fluent calls.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="builder"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// If <paramref name="callbackPrefix"/> is <c>null</c>, empty or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="totalPages"/> is less than <c>1</c>, or if
    /// <paramref name="currentPage"/> is outside the range <c>1..totalPages</c>.
    /// </exception>
    public static InlineKeyboardBuilder AddPaginationRow(
        this InlineKeyboardBuilder builder,
        int currentPage,
        int totalPages,
        string callbackPrefix)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        if (string.IsNullOrWhiteSpace(callbackPrefix))
            throw new ArgumentException("Callback prefix cannot be null, empty or whitespace.", nameof(callbackPrefix));

        if (totalPages < 1)
            throw new ArgumentOutOfRangeException(nameof(totalPages), "Total pages must be at least 1.");

        if (currentPage < 1 || currentPage > totalPages)
            throw new ArgumentOutOfRangeException(nameof(currentPage), $"Current page must be between 1 and {totalPages}.");

        // Start a new row for the pagination controls
        builder.NewRow();

        // ---- Prev button (if applicable) ----
        if (currentPage > 1)
        {
            var prevCallback = $"{callbackPrefix}_prev";
            builder.AddButton("« Prev", prevCallback);
        }
        else
        {
            // Disabled placeholder – an em‑dash looks like a non‑clickable spacer
            builder.AddButton("—", string.Empty);
        }

        // ---- Counter label (non‑interactive) ----
        var counterLabel = $"Page {currentPage}/{totalPages}";
        builder.AddButton(counterLabel, string.Empty);

        // ---- Next button (if applicable) ----
        if (currentPage < totalPages)
        {
            var nextCallback = $"{callbackPrefix}_next";
            builder.AddButton("Next »", nextCallback);
        }
        else
        {
            builder.AddButton("—", string.Empty);
        }

        return builder;
    }
}
