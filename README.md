# ShortTools.12MagicContainer
This project was inspired by "Pezzza's Work" on YouTube, specifically this 
[Video](https://youtu.be/L4xOCvELWlU?si=x7v8hrSUxCz1ISot). The main benefit of using this container rather than another container such as a list is that this container has a O(1) access, deletion, and insertion. This makes it ideal for games where you are constantly removing and adding enemies, especially if the amount of enemies stays roughly consistent and the size of the enemy class is significantly bigger than 2 integers.

This container does require 2 integers to be allocated per item, meaning that if the memory is a significant worry and the size of the class is small, this may not be ideal.

Usage should be very similar to any other collection, especially a list.

Note that if you remove an object at index i, that index will be a deleted index, so if you try and access it again it will error.

Iterating through this collection is a thread safe operation, allowing you to iterate through it while another thread modifies it.

Any suggestions please let me know at matthew.short.nh@gmail.com
