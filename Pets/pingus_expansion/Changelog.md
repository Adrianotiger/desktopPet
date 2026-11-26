# Pingus Expansion



### Detailed Changelog



v0.1 (2026-05-12)

* Changed "walkup" animation to trigger at vertical borders
* Changed wait/block animation at vertical border to a simple flip
* Made Miner trigger at vertical borders only and mine offscreen, respawning after
* Made Basher trigger at vertical borders only and attempt to bash through the wall but give up
* Made balloons ease upwards and fade away after Pingu lands
* Added dragging animation
* Revamped falling

  * Made Pingu hop up and then ease into falling
  * When Pingu reaches terminal velocity, it switches to a newly imported tumble animation, just like in the original Pingus
  * Made drown/splat\* animation only happen if the Pingu was tumbling; otherwise it just walks away unscathed
* Made bullets move up and down while flying to look more dynamic
* Made Digger dig through windows and the taskbar
* \*Added splat animation with newly imported sprites
* Balancing and tweaking



v0.2 (2026-05-20)

* Fixed bullet premature exploding by making the explosion trigger at vertical borders only
* Halved walk animation repeat amount and inversely increased its probability
* Imported bomber animation for when you kill them
* Created a super rare animation...
* Other tweaks



v0.3 (2026-06-11)

* Added subtle recoil to rocket launcher
* Added angel animation
* Added sprinting (+ new death animation)
* Added another rare event...
* Other tweaks



v0.4 (2026-07-31)

* Added jump that triggers if Pingu touches the ground right after being dragged (and also when you drop it below the taskbar)
* Added 2 new uncommon animations
* Added rare easter egg
* Added extremely rare event when making Pingu jump...
* New icon (finally)



v1.0 (2026-09-22)

* Added custom tossing animations! (toss, fall soft, fall hard)
* They now collide with windows and they can shoot each other >:)
* Added lotsa sound effects (31 of them!!)

v1.0.1 (2026-10-20)

* removed sprint sfx
* skate sfx happens 100% of the time on the first time only
* better icon
* updated README
* much lower chance to moonwalk
* quieter fly/jackhammer sfx
* halved chance for upwards balloon anim



v2.0 (2026-11-23)

* WAYY more animation possibilities using the new pet interaction system. Pingus can now:

  * Dodge incoming bullets by sliding under them, and even deflect bullets if powerful enough
  * Turn around if they meet a Stopper Pingu (unless they're fast enough...)
  * Get annoyed at each other for skateboarding
  * Get swept off their feet when another Pingu slides under them
  * Moonwalk together more consistently
  * Etc.
* Other new animations:

  * \*An exit cave can spawn for the Pingus to enter! Finally they can escape each other's wrath
  * Pingus can fall off window edges (finally)
  * You can knock off Christmas Pingus' hats :'(
* Ported other old interaction anims to the new system
* Rare events now work when Pingus are facing to the right :D
* Stopper lasts way longer and also doesn't randomly explode lmao
* Pingus can get hit by bullets during almost every animation, rather than just walking
* Increased chance to shoot and to be a stopper, and decreased chance to moonwalk
* Many other adjustments

v2.0.1

* Made cave spawn consistent between screen sizes. Thank you Adriano for help with the formula!

